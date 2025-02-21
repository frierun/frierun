using Frierun.Server.Data;
using Frierun.Server.Services;

namespace Frierun.Server.Installers.Docker;

public class VolumeInstaller(DockerService dockerService) : IInstaller<Volume>, IUninstaller<DockerVolume>
{
    /// <inheritdoc />
    public Contract Initialize(Volume contract, ExecutionPlan plan)
    {
        if (contract.VolumeName != null)
        {
            return contract;
        }
        
        var baseName = plan.Prefix + (contract.Name == "" ? "" : $"-{contract.Name}");
        
        var count = 0;
        var name = baseName;
        while (plan.State.Resources.OfType<DockerVolume>().Any(c => c.Name == name))
        {
            count++;
            name = $"{baseName}{count}";
        }

        return contract with
            {
                VolumeName = name
            };
    }    
    
    /// <inheritdoc />
    public Resource Install(Volume contract, ExecutionPlan plan)
    {
        var volumeName = contract.VolumeName!;
        var existingVolume = plan.State
            .Resources
            .OfType<DockerVolume>()
            .FirstOrDefault(dockerVolume => dockerVolume.Name == volumeName);

        if (existingVolume != null)
        {
            return existingVolume;
        }
        
        dockerService.CreateVolume(volumeName).Wait();
        return new DockerVolume(volumeName);
    }

    /// <inheritdoc />
    void IUninstaller<DockerVolume>.Uninstall(DockerVolume resource)
    {
        dockerService.RemoveVolume(resource.Name).Wait();
    }
}