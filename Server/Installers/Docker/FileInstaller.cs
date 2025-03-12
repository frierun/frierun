using Docker.DotNet.Models;
using Frierun.Server.Data;
using Frierun.Server.Services;
using File = Frierun.Server.Data.File;
using Mount = Docker.DotNet.Models.Mount;

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

        var containerId = dockerService.StartContainer(
            new CreateContainerParameters
            {
                Image = "alpine:latest",
                Cmd = ["tail", "-f", "/dev/null"],
                HostConfig = new HostConfig
                {
                    Mounts = new List<Mount>
                    {
                        new()
                        {
                            Source = volume.Name,
                            Target = "/mnt",
                            Type = "volume",
                        }
                    }
                }
            }
        ).Result;

        if (containerId == null)
        {
            throw new Exception("Failed to start container");
        }

        var path = $"/mnt/{contract.Path}";

        if (contract.Text != null)
        {
            dockerService.PutFile(containerId, path, contract.Text).Wait();
        }

        if (contract.Owner != null)
        {
            dockerService.ExecInContainer(containerId, ["chown", contract.Owner.ToString() ?? "0", path]).Wait();
        }

        if (contract.Group != null)
        {
            dockerService.ExecInContainer(containerId, ["chgrp", contract.Group.ToString() ?? "0", path]).Wait();
        }

        dockerService.RemoveContainer(containerId).Wait();

        return null;
    }
}