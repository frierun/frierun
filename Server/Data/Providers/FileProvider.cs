using Frierun.Server.Services;

namespace Frierun.Server.Data;

public class FileProvider(DockerService dockerService) : IInstaller<File>
{
    /// <inheritdoc />
    public IEnumerable<ContractDependency> Dependencies(File contract, ExecutionPlan plan)
    {
        yield return new ContractDependency(
            new Volume(contract.VolumeName),
            contract
        );
    }

    /// <inheritdoc />
    public Resource? Install(File contract, ExecutionPlan plan)
    {
        var volume = plan.GetResource<DockerVolume>(contract.VolumeId);
        
        dockerService.PutFile(volume.Name, contract.Path, contract.Text).Wait();
        return null;
    }
}