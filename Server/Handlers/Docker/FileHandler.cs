using Docker.DotNet.Models;
using Frierun.Server.Data;
using File = Frierun.Server.Data.File;
using Mount = Docker.DotNet.Models.Mount;

namespace Frierun.Server.Handlers.Docker;

public class FileHandler(Application application, DockerService dockerService) : Handler<File>(application)
{
    public override IEnumerable<ContractInitializeResult> Initialize(File contract, string prefix)
    {
        yield return new ContractInitializeResult(
            contract with
            {
                Handler = this,
                DependsOn = contract.DependsOn.Append(contract.Volume)
            }
        );
    }

    public override File Install(File contract, ExecutionPlan plan)
    {
        var volume = plan.GetContract(contract.Volume);
        Mount? mount;
        if (volume.VolumeName != null)
        {
            mount = new Mount
            {
                Source = volume.VolumeName,
                Target = "/mnt",
                Type = "volume",
            };
        }
        else if (volume.LocalPath != null)
        {
            mount = new Mount
            {
                Source = volume.LocalPath,
                Target = "/mnt",
                Type = "bind",
            };
        }
        else
        {
            throw new Exception($"Unknown volume type for volume {volume.Name}");
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