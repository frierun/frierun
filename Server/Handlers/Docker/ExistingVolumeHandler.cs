using System.Diagnostics;
using Frierun.Server.Data;

namespace Frierun.Server.Handlers.Docker;

public class ExistingVolumeHandler(Application application, DockerService dockerService)
    : Handler<Volume>(application)
{
    public override IEnumerable<ContractInitializeResult> Initialize(Volume contract, string prefix)
    {
        if (contract.LocalPath != null)
        {
            yield break;
        }

        var volumeName = contract.VolumeName ?? State.Contracts
            .OfType<Volume>()
            .FirstOrDefault(volume => volume.VolumeName != null)?.VolumeName;
        
        if (volumeName == null)
        {
            yield break;
        }

        if (State.Contracts.OfType<Volume>().All(volume => volume.VolumeName != volumeName))
        {
            yield break;
        }

        yield return new ContractInitializeResult(
            contract with
            {
                Handler = this,
                VolumeName = volumeName
            }
        );
    }

    public override Volume Install(Volume contract, ExecutionPlan plan)
    {
        Debug.Assert(contract.LocalPath == null);
        return contract;
    }

    public override void Uninstall(Volume contract)
    {
        Debug.Assert(contract.VolumeName != null);

        var volumeUsed = State.Contracts
            .OfType<Volume>()
            .Count(volume => volume.VolumeName == contract.VolumeName);

        if (volumeUsed > 1)
        {
            return;
        }

        dockerService.RemoveVolume(contract.VolumeName).Wait();
    }
}