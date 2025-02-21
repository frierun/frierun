using Docker.DotNet.Models;
using Frierun.Server.Data;
using Frierun.Server.Services;
using Network = Frierun.Server.Data.Network;

namespace Frierun.Server.Installers.Docker;

public class ContainerInstaller(DockerService dockerService) : IInstaller<Container>, IUninstaller<DockerContainer>
{
    /// <inheritdoc />
    public IEnumerable<ContractDependency> Dependencies(Container contract, ExecutionPlan plan)
    {
        yield return new ContractDependency(
            new Network(contract.NetworkName),
            contract
        );
    }

    /// <inheritdoc />
    public Contract Initialize(Container contract, ExecutionPlan plan)
    {
        var baseName = contract.ContainerName ?? plan.Prefix + (contract.Name == "" ? "" : $"-{contract.Name}");
        
        var count = 1;
        var name = baseName;
        while (plan.State.Resources.OfType<DockerContainer>().Any(c => c.Name == name))
        {
            count++;
            name = $"{baseName}{count}";
        }

        return contract.ContainerName == name
            ? contract
            : contract with
            {
                ContainerName = name
            };
    }

    /// <inheritdoc />
    public Resource Install(Container contract, ExecutionPlan plan)
    {
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
            Labels = new Dictionary<string, string>(),
            Name = contract.ContainerName!,
            NetworkingConfig = new NetworkingConfig()
            {
                EndpointsConfig = new Dictionary<string, EndpointSettings>()
            }
        };

        if (contract.RequireDocker)
        {
            dockerParameters.HostConfig.Mounts.Add(new global::Docker.DotNet.Models.Mount
            {
                Source = "/var/run/docker.sock",
                Target = "/var/run/docker.sock",
                Type = "bind"
            });
        }

        foreach (var action in contract.Configure)
        {
            action(dockerParameters);
        }
        
        var result = dockerService.StartContainer(dockerParameters).Result;

        if (!result)
        {
            throw new Exception("Failed to start container");
        }

        return new DockerContainer(contract.ContainerName!)
        {
            DependsOn = plan.GetPrerequisites(contract.Id).Select(plan.GetResource).OfType<Resource>().ToList()
        };
    }

    /// <inheritdoc />
    void IUninstaller<DockerContainer>.Uninstall(DockerContainer resource)
    {
        dockerService.StopContainer(resource.Name).Wait();
    }
}