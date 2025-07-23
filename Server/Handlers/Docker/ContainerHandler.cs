using System.Diagnostics;
using Docker.DotNet.Models;
using Frierun.Server.Data;
using Mount = Docker.DotNet.Models.Mount;

namespace Frierun.Server.Handlers.Docker;

public class ContainerHandler(Application application, DockerService dockerService)
    : Handler<Container>(application), IContainerHandler
{
    private readonly DockerApiConnection _dockerApiConnection =
        application.Contracts.OfType<DockerApiConnection>().Single();

    public override IEnumerable<ContractInitializeResult> Initialize(Container contract, string prefix)
    {
        yield return new ContractInitializeResult(
            contract with
            {
                ContainerName = contract.ContainerName ?? FindUniqueName(
                    prefix + (contract.Name == "" ? "" : $"-{contract.Name}"),
                    c => c.ContainerName
                ),
                Labels = new Dictionary<string, string>(contract.Labels)
                {
                    ["com.docker.compose.project"] = prefix,
                    ["com.docker.compose.service"] = contract.Name
                },
                DependsOn = contract.DependsOn
                    .Append(contract.Network)
                    .Concat(contract.Mounts.Values.Select(mount => mount.Volume)),
                Handler = this
            }
        );
    }

    public override Container Install(Container contract, ExecutionPlan plan)
    {
        var network = plan.GetContract(contract.Network);
        Debug.Assert(network.Installed);
        Debug.Assert(contract.ContainerName != null);

        var dockerParameters = new CreateContainerParameters
        {
            Cmd = contract.Command.ToList(),
            Env = contract.Env.Select(kv => $"{kv.Key}={kv.Value}").ToList(),
            Image = contract.ImageName,
            HostConfig = new HostConfig
            {
                RestartPolicy = new RestartPolicy
                {
                    Name = RestartPolicyKind.UnlessStopped
                },
                Mounts = new List<Mount>(),
                PortBindings = new Dictionary<string, IList<PortBinding>>()
            },
            Labels = new Dictionary<string, string>(contract.Labels),
            Name = contract.ContainerName,
            NetworkingConfig = new NetworkingConfig
            {
                EndpointsConfig = new Dictionary<string, EndpointSettings>
                {
                    {
                        network.NetworkName, new EndpointSettings
                        {
                            Aliases = contract.Name == "" ? Array.Empty<string>() : new List<string> { contract.Name }
                        }
                    }
                }
            }
        };

        // docker socket
        if (contract.MountDockerSocket)
        {
            dockerParameters.HostConfig.Mounts.Add(
                new Mount
                {
                    Source = _dockerApiConnection.GetSocketRootPath(),
                    Target = "/var/run/docker.sock",
                    Type = "bind"
                }
            );
        }

        // mounts
        foreach (var (path, mount) in contract.Mounts)
        {
            var volume = plan.GetContract(mount.Volume);
            Debug.Assert(volume.Installed);
            
            var dockerMount = new Mount
            {
                Target = path,
                ReadOnly = mount.ReadOnly
            };
            
            if (volume.VolumeName != null)
            {
                dockerMount.Source = volume.VolumeName;
                dockerMount.Type = "volume";
            }
            else if (volume.LocalPath != null)
            {
                dockerMount.Source = volume.LocalPath;
                dockerMount.Type = "bind";
            }
            else
            {
                throw new Exception($"Volume {volume} has no name or local path");
            }

            dockerParameters.HostConfig.Mounts.Add(dockerMount);
        }
        
        // exposes ports
        var endpoints = plan.Contracts.OfType<PortEndpoint>().Where(ep => ep.Container == contract);
        foreach (var endpoint in endpoints)
        {
            Debug.Assert(endpoint.Installed);
            dockerParameters.HostConfig.PortBindings[$"{endpoint.Port}/{endpoint.Protocol.ToString().ToLower()}"] =
                new List<PortBinding>
                {
                    new()
                    {
                        HostPort = endpoint.ExternalPort.ToString()
                    }
                };            
        }

        var result = dockerService.StartContainer(dockerParameters).Result;

        if (result == null)
        {
            throw new Exception("Failed to start container");
        }

        return contract with { NetworkName = network.NetworkName };
    }

    public override void Uninstall(Container container)
    {
        Debug.Assert(container.Installed);
        dockerService.RemoveContainer(container.ContainerName).Wait();
    }

    public void AttachNetwork(Container container, string networkName)
    {
        Debug.Assert(container.Installed);
        dockerService.AttachNetwork(networkName, container.ContainerName).Wait();
    }

    public void DetachNetwork(Container container, string networkName)
    {
        Debug.Assert(container.Installed);
        dockerService.DetachNetwork(networkName, container.ContainerName).Wait();
    }

    public (string stdout, string stderr) ExecInContainer(Container container, IList<string> command)
    {
        Debug.Assert(container.Installed);
        return dockerService.ExecInContainer(container.ContainerName, command).Result;
    }
}