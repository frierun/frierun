using Frierun.Server.Services;

namespace Frierun.Server.Data;

public class VolumeProvider(DockerService dockerService) : IInstaller<VolumeContract>, IUninstaller<Volume>
{
    /// <inheritdoc />
    public IEnumerable<ContractDependency> Dependencies(VolumeContract contract, ExecutionPlan plan)
    {
        yield break;
    }

    /// <inheritdoc />
    public Contract Initialize(VolumeContract contract, ExecutionPlan plan)
    {
        var baseName = contract.VolumeName ?? plan.Prefix + (contract.Name == "" ? "" : $"-{contract.Name}");
        
        var count = 0;
        var name = baseName;
        while (plan.State.Resources.OfType<Volume>().Any(c => c.Name == name))
        {
            count++;
            name = $"{baseName}{count}";
        }

        return contract.VolumeName == name
            ? contract
            : contract with
            {
                VolumeName = name
            };
    }    
    
    /// <inheritdoc />
    public Resource Install(VolumeContract contract, ExecutionPlan plan)
    {
        var volumeName = contract.VolumeName!;
        
        dockerService.CreateVolume(volumeName).Wait();
        return new Volume(volumeName);
    }

    /// <inheritdoc />
    void IUninstaller<Volume>.Uninstall(Volume resource)
    {
        dockerService.RemoveVolume(resource.Name).Wait();
    }
}