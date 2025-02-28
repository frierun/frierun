using Frierun.Server.Data;
using Frierun.Server.Services;

namespace Frierun.Server.Installers.Docker;

public class VolumeInstaller(DockerService dockerService) : IInstaller<Volume>, IUninstaller<DockerVolume>
{
    /// <inheritdoc />
    InstallerInitializeResult IInstaller<Volume>.Initialize(Volume contract, string prefix, State state)
    {
        if (contract.VolumeName != null)
        {
            return new InstallerInitializeResult(contract);
        }

        var baseName = prefix + (contract.Name == "" ? "" : $"-{contract.Name}");

        var count = 0;
        var name = baseName;
        while (state.Resources.OfType<DockerVolume>().Any(c => c.Name == name))
        {
            count++;
            name = $"{baseName}{count}";
        }

        return new InstallerInitializeResult(
            contract with
            {
                VolumeName = name
            }
        );
    }

    /// <inheritdoc />
    Resource IInstaller<Volume>.Install(Volume contract, ExecutionPlan plan)
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