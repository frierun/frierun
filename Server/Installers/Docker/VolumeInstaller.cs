using Frierun.Server.Data;

namespace Frierun.Server.Installers.Docker;

public class VolumeInstaller(DockerService dockerService, State state) : IInstaller<Volume>, IUninstaller<DockerVolume>
{
    /// <inheritdoc />
    public Application? Application => null;

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
            return new LocalPath{Path = contract.Path};
        }

        var volumeName = contract.VolumeName!;

        if (state.Resources.OfType<DockerVolume>().All(dockerVolume => dockerVolume.Name != volumeName))
        {
            dockerService.CreateVolume(volumeName).Wait();
        }

        return new DockerVolume { Name = volumeName };
    }

    /// <inheritdoc />
    void IUninstaller<DockerVolume>.Uninstall(DockerVolume resource)
    {
        if (state.Resources.OfType<DockerVolume>().Count(dockerVolume => dockerVolume.Name == resource.Name) > 1)
        {
            return;
        }

        dockerService.RemoveVolume(resource.Name).Wait();
    }
}