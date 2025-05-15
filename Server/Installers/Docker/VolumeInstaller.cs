using Frierun.Server.Data;
using Frierun.Server.Installers.Base;

namespace Frierun.Server.Installers.Docker;

public class VolumeInstaller(Application application, DockerService dockerService, State state)
    : IInstaller<Volume>, IHandler<DockerVolume>
{
    public Application Application => application;

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
        while (state.Contracts
               .OfType<Volume>()
               .Select(volume => volume.Result)
               .OfType<DockerVolume>()
               .Any(c => c.Name == name)
              )
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

    Volume IInstaller<Volume>.Install(Volume contract, ExecutionPlan plan)
    {
        if (contract.Path != null)
        {
            return contract with
            {
                Result = new LocalPath(new EmptyHandler()) { Path = contract.Path }
            };
        }

        var volumeName = contract.VolumeName!;

        if (state.Contracts
            .OfType<Volume>()
            .Select(volume => volume.Result)
            .OfType<DockerVolume>()
            .All(dockerVolume => dockerVolume.Name != volumeName)
           )
        {
            dockerService.CreateVolume(volumeName).Wait();
        }

        return contract with
        {
            Result = new DockerVolume(this) { Name = volumeName }
        };
    }

    void IHandler<DockerVolume>.Uninstall(DockerVolume resource)
    {
        var volumeUsed = state.Contracts
            .OfType<Volume>()
            .Select(volume => volume.Result)
            .OfType<DockerVolume>()
            .Count(dockerVolume => dockerVolume.Name == resource.Name);
        
        if (volumeUsed > 1)
        {
            return;
        }

        dockerService.RemoveVolume(resource.Name).Wait();
    }
}