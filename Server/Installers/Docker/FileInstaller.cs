using Frierun.Server.Data;
using Frierun.Server.Services;
using File = Frierun.Server.Data.File;

namespace Frierun.Server.Installers.Docker;

public class FileInstaller(DockerService dockerService) : IInstaller<File>
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

        if (contract.Path == null)
        {
            throw new Exception("File path is not set.");
        }
        
        dockerService.PutFile(volume.Name, contract.Path, contract.Text).Wait();
        return null;
    }
}