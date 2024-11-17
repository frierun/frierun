using Docker.DotNet.Models;
using Frierun.Server.Services;

namespace Frierun.Server.Data;

public class NetworkProvider(DockerService dockerService) : Provider<Network, NetworkContract>
{
    /// <inheritdoc />
    protected override Network Install(NetworkContract contract, ExecutionPlan plan)
    {
        var networkName = plan.GetPrefixedName(contract.Name);

        dockerService.CreateNetwork(networkName).Wait();

        var containerGroup = new Network(networkName);

        foreach (var containerContract in plan.GetResourcesOfType<ContainerContract>())
        {
            plan.UpdateContract(
                containerContract with
                {
                    Configure = containerContract.Configure.Append(
                        parameters =>
                        {
                            parameters.Labels["com.docker.compose.project"] = plan.Prefix;
                            parameters.Labels["com.docker.compose.service"] = containerContract.Name;

                            parameters.NetworkingConfig.EndpointsConfig = new Dictionary<string, EndpointSettings>
                            {
                                {
                                    networkName, new EndpointSettings()
                                    {
                                        Aliases = new List<string> { containerContract.Name }
                                    }
                                }
                            };
                        }
                    ),
                    DependsOn = containerContract.DependsOn.Append(containerGroup)
                }
            );
        }

        return containerGroup;
    }

    /// <inheritdoc />
    protected override void Uninstall(Network resource)
    {
        dockerService.RemoveNetwork(resource.Name).Wait();
    }
}