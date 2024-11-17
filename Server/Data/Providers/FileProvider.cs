using Frierun.Server.Services;
using File = Frierun.Server.Data.File;

namespace Frierun.Server.Data;

public class FileProvider(DockerService dockerService) : Provider<File, FileContract>
{
    /// <inheritdoc />
    protected override IEnumerable<ContractDependency> Dependencies(FileContract contract, ExecutionPlan plan)
    {
        yield return new ContractDependency(
            new VolumeContract(contract.VolumeName),
            contract
        );
    }
    
    /// <inheritdoc />
    protected override File Install(FileContract contract, ExecutionPlan plan)
    {
        var volume = plan.GetResource<Volume>(contract.VolumeId);
        
        dockerService.PutFile(volume.Name, contract.Path, contract.Text).Wait();

        return new File
        {
            DependsOn = [volume]
        };
    }
}