using Docker.DotNet.Models;
using Frierun.Server.Services;

namespace Frierun.Server.Data;

public class ContainerProvider(DockerService dockerService) : Provider<Container, ContainerContract>
{
    /// <inheritdoc />
    protected override IEnumerable<ContractDependency> Dependencies(ContainerContract contract, ExecutionPlan plan)
    {
        yield return new ContractDependency(
            new NetworkContract(contract.NetworkName),
            contract
        );
    }

    /// <inheritdoc />
    protected override Container Install(ContainerContract contract, ExecutionPlan plan)
    {
        var name = plan.GetPrefixedName(contract.Name);
        
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
            Name = name,
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

        return new Container(name)
        {
            DependsOn = contract.DependsOn.ToList()
        };
    }
    
    /// <inheritdoc />
    protected override void Uninstall(Container resource)
    {
        dockerService.StopContainer(resource.Name).Wait();
    }
}