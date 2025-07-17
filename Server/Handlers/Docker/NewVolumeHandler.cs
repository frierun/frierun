using System.Diagnostics;
using Frierun.Server.Data;

namespace Frierun.Server.Handlers.Docker;

public class NewVolumeHandler(Application application, DockerService dockerService)
    : Handler<Volume>(application)
{
    public override IEnumerable<ContractInitializeResult> Initialize(Volume contract, string prefix)
    {
        if (contract.LocalPath != null)
        {
            yield break;
        }

        if (contract.VolumeName != null && State.Contracts
                .OfType<Volume>()
                .Any(volume => volume.VolumeName == contract.VolumeName))
        {
            yield break;
        }

        yield return new ContractInitializeResult(
            contract with
            {
                Handler = this,
                VolumeName = contract.VolumeName ?? FindUniqueName(
                    prefix + (contract.Name == "" ? "" : $"-{contract.Name}"),
                    volume => volume.VolumeName
                )
            }
        );
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