using Docker.DotNet.Models;
using Frierun.Server.Services;

namespace Frierun.Server.Data;

public class ContainerProvider(DockerService dockerService) : IInstaller<ContainerContract>, IUninstaller<Container>
{
    /// <inheritdoc />
    public IEnumerable<ContractDependency> Dependencies(ContainerContract contract, ExecutionPlan plan)
    {
        yield return new ContractDependency(
            new NetworkContract(contract.NetworkName),
            contract
        );
    }

    /// <inheritdoc />
    public Contract Initialize(ContainerContract contract, ExecutionPlan plan)
    {
        var baseName = contract.ContainerName ?? plan.Prefix + (contract.Name == "" ? "" : $"-{contract.Name}");
        
        var count = 1;
        var name = baseName;
        while (plan.State.Resources.OfType<Container>().Any(c => c.Name == name))
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
    public Resource Install(ContainerContract contract, ExecutionPlan plan)
    {
        var dockerParameters = new CreateContainerParameters
        {
            Cmd = contract.Command.ToList(),
            Env = contract.Env.Select(kv => $"{kv.Key}={kv.Value}").ToList(),
            Image = contract.ImageName,
            HostConfig = new HostConfig
            {
                Mounts = new List<Docker.DotNet.Models.Mount>(),
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
            dockerParameters.HostConfig.Mounts.Add(new Docker.DotNet.Models.Mount
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

        return new Container(contract.ContainerName!)
        {
            DependsOn = contract.DependsOn.ToList()
        };
    }

    /// <inheritdoc />
    void IUninstaller<Container>.Uninstall(Container resource)
    {
        dockerService.StopContainer(resource.Name).Wait();
    }
}