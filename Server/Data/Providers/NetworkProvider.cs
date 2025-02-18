using Docker.DotNet.Models;
using Frierun.Server.Services;

namespace Frierun.Server.Data;

public class NetworkProvider(DockerService dockerService) : IInstaller<Network>, IUninstaller<DockerNetwork>
{
    /// <inheritdoc />
    public Contract Initialize(Network contract, ExecutionPlan plan)
    {
        var baseName = contract.NetworkName ?? plan.Prefix + (contract.Name == "" ? "" : $"-{contract.Name}");
        
        var count = 1;
        var name = baseName;
        while (plan.State.Resources.OfType<DockerNetwork>().Any(c => c.Name == name))
        {
            count++;
            name = $"{baseName}{count}";
        }

        return contract.NetworkName == name
            ? contract
            : contract with
            {
                NetworkName = name
            };
    }
    
    /// <inheritdoc />
    public Resource? Install(Network contract, ExecutionPlan plan)
    {
        var networkName = contract.NetworkName!;

        dockerService.CreateNetwork(networkName).Wait();

        var network = new DockerNetwork(networkName);
        foreach (var containerContract in plan.GetResourcesOfType<Container>())
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
                                    networkName, new EndpointSettings
                                    {
                                        Aliases = new List<string> { containerContract.Name }
                                    }
                                }
                            };
                        }
                    ),
                }
            );
        }

        return network;
    }

    /// <inheritdoc />
    void IUninstaller<DockerNetwork>.Uninstall(DockerNetwork resource)
    {
        dockerService.RemoveNetwork(resource.Name).Wait();
    }
}