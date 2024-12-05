using Frierun.Server.Services;

namespace Frierun.Server.Data;

public class VolumeProvider(DockerService dockerService) : IInstaller<Volume>, IUninstaller<DockerVolume>
{
    /// <inheritdoc />
    public Contract Initialize(Volume contract, ExecutionPlan plan)
    {
        var baseName = contract.VolumeName ?? plan.Prefix + (contract.Name == "" ? "" : $"-{contract.Name}");
        
        var count = 0;
        var name = baseName;
        while (plan.State.Resources.OfType<DockerVolume>().Any(c => c.Name == name))
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
    public Resource Install(Volume contract, ExecutionPlan plan)
    {
        var volumeName = contract.VolumeName!;
        
        dockerService.CreateVolume(volumeName).Wait();
        return new DockerVolume(volumeName);
    }

    /// <inheritdoc />
    void IUninstaller<DockerVolume>.Uninstall(DockerVolume resource)
    {
        dockerService.RemoveVolume(resource.Name).Wait();
    }
}