using System.Diagnostics;
using Docker.DotNet.Models;
using Frierun.Server.Data;

namespace Frierun.Server.Handlers.Docker;

public class ContainerHandler(Application application, DockerService dockerService, State state)
    : IContainerHandler
{
    public Application Application => application;

    public IEnumerable<ContractInitializeResult> Initialize(Container contract, string prefix)
    {
        var baseName = contract.ContainerName ?? prefix + (contract.Name == "" ? "" : $"-{contract.Name}");

        var count = 1;
        var name = baseName;
        while (state.Contracts.OfType<Container>().Any(c => c.ContainerName == name))
        {
            count++;
            name = $"{baseName}{count}";
        }

        yield return new ContractInitializeResult(
            contract with
            {
                ContainerName = name,
                DependsOn = contract.DependsOn.Append(contract.Network),
                Handler = this
            }
        );
    }

    public Container Install(Container contract, ExecutionPlan plan)
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
                Mounts = new List<global::Docker.DotNet.Models.Mount>(),
                PortBindings = new Dictionary<string, IList<PortBinding>>()
            },
            Labels = new Dictionary<string, string>
            {
                ["com.docker.compose.project"] = network.NetworkName,
                ["com.docker.compose.service"] = contract.Name
            },
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

        if (contract.RequireDocker)
        {
            dockerParameters.HostConfig.Mounts.Add(
                new global::Docker.DotNet.Models.Mount
                {
                    Source = "/var/run/docker.sock",
                    Target = "/var/run/docker.sock",
                    Type = "bind"
                }
            );
        }

        foreach (var action in contract.Configure)
        {
            action(dockerParameters);
        }

        var result = dockerService.StartContainer(dockerParameters).Result;

        if (result == null)
        {
            throw new Exception("Failed to start container");
        }

        return contract with { NetworkName = network.NetworkName };
    }

    void IHandler<Container>.Uninstall(Container container)
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

    public Task<(string stdout, string stderr)> ExecInContainer(Container container, IList<string> command)
    {
        Debug.Assert(container.Installed);
        return dockerService.ExecInContainer(container.ContainerName, command);
    }
}