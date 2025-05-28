using System.Diagnostics;
using Frierun.Server.Data;

namespace Frierun.Server.Handlers.Docker;

public class VolumeHandler(Application application, DockerService dockerService, State state)
    : IHandler<Volume>
{
    public Application Application => application;

    public IEnumerable<ContractInitializeResult> Initialize(Volume contract, string prefix)
    {
        if (contract.LocalPath != null)
        {
            yield break;
        }
        
        if (contract.VolumeName != null)
        {
            yield return new ContractInitializeResult(contract with { Handler = this });
            yield break;
        }

        var baseName = prefix + (contract.Name == "" ? "" : $"-{contract.Name}");

        var count = 0;
        var name = baseName;
        while (state.Contracts
               .OfType<Volume>()
               .Any(volume => volume.VolumeName == name)
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
        Debug.Assert(contract.LocalPath == null);
        
        var volumeName = contract.VolumeName!;

        if (state.Contracts
            .OfType<Volume>()
            .All(volume => volume.VolumeName != volumeName)
           )
        {
            dockerService.CreateVolume(volumeName).Wait();
        }

        return contract;
    }

    public void Uninstall(Volume contract)
    {
        Debug.Assert(contract.VolumeName != null);

        var volumeUsed = state.Contracts
            .OfType<Volume>()
            .Count(volume => volume.VolumeName == contract.VolumeName);

        if (volumeUsed > 1)
        {
            return;
        }

        dockerService.RemoveVolume(contract.VolumeName).Wait();
    }
}