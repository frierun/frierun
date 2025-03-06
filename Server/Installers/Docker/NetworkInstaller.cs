using Docker.DotNet.Models;
using Frierun.Server.Data;
using Frierun.Server.Services;
using Network = Frierun.Server.Data.Network;

namespace Frierun.Server.Installers.Docker;

public class NetworkInstaller(DockerService dockerService) : IInstaller<Network>, IUninstaller<DockerNetwork>
{
    /// <inheritdoc />
    IEnumerable<InstallerInitializeResult> IInstaller<Network>.Initialize(Network contract, string prefix, State state)
    {
        var baseName = contract.NetworkName ?? prefix + (contract.Name == "" ? "" : $"-{contract.Name}");
        
        var count = 1;
        var name = baseName;
        while (state.Resources.OfType<DockerNetwork>().Any(c => c.Name == name))
        {
            count++;
            name = $"{baseName}{count}";
        }
        
        yield return new InstallerInitializeResult(
            contract with
            {
                NetworkName = name
            }
        );
    }

    /// <inheritdoc />
    Resource IInstaller<Network>.Install(Network contract, ExecutionPlan plan)
    {
        var networkName = contract.NetworkName!;

        dockerService.CreateNetwork(networkName).Wait();

        foreach (var containerContract in plan.GetContractsOfType<Container>())
        {
            plan.UpdateContract(
                containerContract with
                {
                    Configure = containerContract.Configure.Append(
                        parameters =>
                        {
                            parameters.Labels["com.docker.compose.project"] = networkName;
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

        return new DockerNetwork(networkName);
    }

    /// <inheritdoc />
    void IUninstaller<DockerNetwork>.Uninstall(DockerNetwork resource)
    {
        dockerService.RemoveNetwork(resource.Name).Wait();
    }
}