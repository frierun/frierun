using System.Diagnostics;
using Frierun.Server.Data;

namespace Frierun.Server.Handlers.Docker;

public class VolumeHandler(Application application, DockerService dockerService, State state)
    : IHandler<Volume>
{
    public Application Application => application;

    public IEnumerable<ContractInitializeResult> Initialize(Volume contract, string prefix)
    {
        if (contract.VolumeName != null || contract.Path != null)
        {
            yield return new ContractInitializeResult(contract with { Handler = this });
            yield break;
        }

        var baseName = prefix + (contract.Name == "" ? "" : $"-{contract.Name}");

        var count = 0;
        var name = baseName;
        while (state.Contracts
               .OfType<Volume>()
               .Select(volume => volume.Result)
               .OfType<DockerVolume>()
               .Any(c => c.Name == name)
              )
        {
            count++;
            name = $"{baseName}{count}";
        }

        yield return new ContractInitializeResult(
            contract with
            {
                Handler = this,
                VolumeName = name
            }
        );
    }

    public Volume Install(Volume contract, ExecutionPlan plan)
    {
        if (contract.Path != null)
        {
            return contract with
            {
                Result = new LocalPath { Path = contract.Path }
            };
        }

        var volumeName = contract.VolumeName!;

        if (state.Contracts
            .OfType<Volume>()
            .Select(volume => volume.Result)
            .OfType<DockerVolume>()
            .All(dockerVolume => dockerVolume.Name != volumeName)
           )
        {
            dockerService.CreateVolume(volumeName).Wait();
        }

        return contract with
        {
            Result = new DockerVolume { Name = volumeName }
        };
    }

    public void Uninstall(Volume contract)
    {
        var installedVolume = contract.Result as DockerVolume;
        Debug.Assert(installedVolume != null);

        var volumeUsed = state.Contracts
            .OfType<Volume>()
            .Select(volume => volume.Result)
            .OfType<DockerVolume>()
            .Count(dockerVolume => dockerVolume.Name == installedVolume.Name);

        if (volumeUsed > 1)
        {
            return;
        }

        dockerService.RemoveVolume(installedVolume.Name).Wait();
    }
}