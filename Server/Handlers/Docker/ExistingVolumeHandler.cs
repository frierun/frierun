using System.Diagnostics;
using Frierun.Server.Data;

namespace Frierun.Server.Handlers.Docker;

public class ExistingVolumeHandler(State state, Application application, DockerService dockerService)
    : Handler<Volume>(state, application)
{
    public override IEnumerable<ContractList> Initialize(Volume contract, ApplicationContext context)
    {
        if (contract.LocalPath != null)
        {
            yield break;
        }

        var volumeName = contract.VolumeName ??
                         State.GetContracts<Volume>().FirstOrDefault(volume => volume.VolumeName != null)?.VolumeName;

        if (volumeName == null)
        {
            yield break;
        }

        if (State.GetContracts<Volume>().All(volume => volume.VolumeName != volumeName))
        {
            yield break;
        }

        yield return new ContractList
        {
            [context] = contract with
            {
                Handler = this,
                VolumeName = volumeName
            }
        };
    }

    public override Volume Install(Volume contract, ExecutionPlan plan)
    {
        Debug.Assert(contract.LocalPath == null);
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