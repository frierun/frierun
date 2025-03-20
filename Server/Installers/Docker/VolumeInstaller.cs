using Frierun.Server.Data;

namespace Frierun.Server.Installers.Docker;

public class VolumeInstaller(DockerService dockerService, State state) : IInstaller<Volume>, IUninstaller<DockerVolume>
{
    /// <inheritdoc />
    IEnumerable<InstallerInitializeResult> IInstaller<Volume>.Initialize(Volume contract, string prefix)
    {
        if (contract.VolumeName != null || contract.Path != null)
        {
            yield return new InstallerInitializeResult(contract);
            yield break;
        }

        var baseName = prefix + (contract.Name == "" ? "" : $"-{contract.Name}");

        var count = 0;
        var name = baseName;
        while (state.Resources.OfType<DockerVolume>().Any(c => c.Name == name))
        {
            count++;
            name = $"{baseName}{count}";
        }

        yield return new InstallerInitializeResult(
            contract with
            {
                VolumeName = name
            }
        );
    }

    /// <inheritdoc />
    Resource IInstaller<Volume>.Install(Volume contract, ExecutionPlan plan)
    {
        if (contract.Path != null)
        {
            return new LocalPath(contract.Path);
        }
        
        var volumeName = contract.VolumeName!;
        var existingVolume = state.Resources
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