using Docker.DotNet.Models;
using Frierun.Server.Data;
using File = Frierun.Server.Data.File;
using Mount = Docker.DotNet.Models.Mount;

namespace Frierun.Server.Handlers.Docker;

public class FileHandler(Application application, DockerService dockerService) : IHandler<File>
{
    public Application Application => application;
    
    public IEnumerable<ContractInitializeResult> Initialize(File contract, string prefix)
    {
        yield return new ContractInitializeResult(
            contract with
            {
                Handler = this,
                DependsOn = contract.DependsOn.Append(contract.Volume)
            }
        );
    }

    public File Install(File contract, ExecutionPlan plan)
    {
        var volume = plan.GetResource(contract.Volume);
        Mount? mount = null;
        if (volume is DockerVolume dockerVolume)
        {
            mount = new Mount
            {
                Source = dockerVolume.Name,
                Target = "/mnt",
                Type = "volume",
            };
        }
        else if (volume is LocalPath localPath)
        {
            mount = new Mount
            {
                Source = localPath.Path,
                Target = "/mnt",
                Type = "bind",
            };
        }

        if (mount == null)
        {
            throw new Exception("Unknown volume type: " + volume.GetType().Name);
        }

        var containerId = dockerService.StartContainer(
            new CreateContainerParameters
            {
                Image = "alpine:latest",
                Cmd = ["tail", "-f", "/dev/null"],
                HostConfig = new HostConfig
                {
                    Mounts = new List<Mount> { mount }
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

        return contract;
    }
}