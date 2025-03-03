using Frierun.Server.Data;
using Frierun.Server.Services;
using File = Frierun.Server.Data.File;

namespace Frierun.Server.Installers.Docker;

public class FileInstaller(DockerService dockerService) : IInstaller<File>
{
    /// <inheritdoc />
    IEnumerable<InstallerInitializeResult> IInstaller<File>.Initialize(File contract, string prefix, State state)
    {
        yield return new InstallerInitializeResult(
            contract,
            [contract.VolumeId]
        );
    }

    /// <inheritdoc />
    IEnumerable<ContractDependency> IInstaller<File>.GetDependencies(File contract, ExecutionPlan plan)
    {
        yield return new ContractDependency(contract.VolumeId, contract.Id);
    }

    /// <inheritdoc />
    Resource? IInstaller<File>.Install(File contract, ExecutionPlan plan)
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