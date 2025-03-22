using Docker.DotNet.Models;
using Frierun.Server.Data;

namespace Frierun.Server.Installers.Docker;

public class ContainerInstaller(DockerService dockerService, State state) : IInstaller<Container>, IUninstaller<DockerContainer>
{
    /// <inheritdoc />
    IEnumerable<InstallerInitializeResult> IInstaller<Container>.Initialize(Container contract, string prefix)
    {
        var baseName = contract.ContainerName ?? prefix + (contract.Name == "" ? "" : $"-{contract.Name}");

        var count = 1;
        var name = baseName;
        while (state.Resources.OfType<DockerContainer>().Any(c => c.Name == name))
        {
            count++;
            name = $"{baseName}{count}";
        }

        yield return new InstallerInitializeResult(
            contract with
            {
                ContainerName = name
            },
            [contract.NetworkId]
        );
    }

    /// <inheritdoc />
    IEnumerable<ContractDependency> IInstaller<Container>.GetDependencies(Container contract, ExecutionPlan plan)
    {
        yield return new ContractDependency(contract.NetworkId, contract);
    }

    /// <inheritdoc />
    Resource IInstaller<Container>.Install(Container contract, ExecutionPlan plan)
    {
        var network = plan.GetResource<DockerNetwork>(contract.NetworkId);
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
            Labels = new Dictionary<string, string>()
            {
                ["com.docker.compose.project"] = network.Name,
                ["com.docker.compose.service"] = contract.Name
            },
            Name = contract.ContainerName!,
            NetworkingConfig = new NetworkingConfig()
            {
                EndpointsConfig  = new Dictionary<string, EndpointSettings>
                {
                    {
                        network.Name, new EndpointSettings
                        {
                            Aliases = new List<string> { contract.Name }
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

        return new DockerContainer(Name: contract.ContainerName!, NetworkName: network.Name);
    }

    /// <inheritdoc />
    void IUninstaller<DockerContainer>.Uninstall(DockerContainer resource)
    {
        dockerService.RemoveContainer(resource.Name).Wait();
    }
}