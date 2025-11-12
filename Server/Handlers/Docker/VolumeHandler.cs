using System.Diagnostics;
using Frierun.Server.Data;

namespace Frierun.Server.Handlers.Docker;

public class VolumeHandler(State state, Application application, DockerService dockerService)
    : Handler<Volume>(state, application)
{
    public override IEnumerable<Volume> Discover()
    {
        return dockerService.ListVolumes().Result.Select(volume => new Volume
            {
                VolumeName = volume.Name
            }
        );
    }

    public override IEnumerable<ContractList> Initialize(Volume contract, ApplicationContext context)
    {
        if (contract.LocalPath != null)
        {
            yield break;
        }

        // contract is set
        if (contract.Installed)
        {
            yield return new ContractList { [context] = State.GetContract(contract.Id) };
            yield break;
        }

        // volume name is set
        if (contract.VolumeName != null)
        {
            var installedContract = State.GetContracts<Volume>()
                .FirstOrDefault(volume => volume.VolumeName == contract.VolumeName && volume.Handler == this);
            if (installedContract != null)
            {
                yield return new ContractList { [context] = installedContract };
            }
            else
            {
                yield return new ContractList { [context] = new Volume { Handler = this } };
            }

            yield break;
        }

        var defaultName = context.Prefix + (context.Name == "" ? "" : $"-{context.Name}");
        var defaultVolume = State.GetContracts<Volume>()
            .FirstOrDefault(volume => volume.VolumeName == defaultName && volume.Handler == this);

        // return the same volume if it exists first
        if (defaultVolume != null)
        {
            yield return new ContractList { [context] = defaultVolume };
        }

        // return a new volume
        yield return new ContractList
        {
            [context] = contract with
            {
                Handler = this,
                VolumeName = FindUniqueName(
                    context.Prefix + (context.Name == "" ? "" : $"-{context.Name}"), 
                    volume => volume.VolumeName
                )
            }
        };

        // return all other installed volumes
        foreach (var installedContract in State.GetContracts<Volume>().Where(volume => volume.Handler == this))
        {
            if (installedContract.VolumeName == defaultName)
            {
                continue;
            }

            yield return new ContractList { [context] = installedContract };
        }
    }

    public override Volume Install(Volume contract, ExecutionPlan plan)
    {
        Debug.Assert(contract.LocalPath == null);

        var volumeName = contract.VolumeName!;
        dockerService.CreateVolume(volumeName).Wait();

        return contract;
    }

    public override void Uninstall(Volume contract)
    {
        Debug.Assert(contract.VolumeName != null);

        var volumeUsed = State.GetContracts<Volume>().Count(volume => volume.VolumeName == contract.VolumeName);

        if (volumeUsed > 1)
        {
            return;
        }

        dockerService.RemoveVolume(contract.VolumeName).Wait();
    }
}