using Frierun.Server.Services;

namespace Frierun.Server.Data;

public class FileProvider(DockerService dockerService) : IInstaller<FileContract>
{
    /// <inheritdoc />
    public IEnumerable<ContractDependency> Dependencies(FileContract contract, ExecutionPlan plan)
    {
        yield return new ContractDependency(
            new VolumeContract(contract.VolumeName),
            contract
        );
    }

    /// <inheritdoc />
    public Contract Initialize(FileContract contract, ExecutionPlan plan)
    {
        return contract;
    }

    /// <inheritdoc />
    public Resource? Install(FileContract contract, ExecutionPlan plan)
    {
        var volume = plan.GetResource<Volume>(contract.VolumeId);
        
        dockerService.PutFile(volume.Name, contract.Path, contract.Text).Wait();
        return null;
    }
}