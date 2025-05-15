using System.Diagnostics;
using Frierun.Server.Data;
using Network = Frierun.Server.Data.Network;

namespace Frierun.Server.Installers.Docker;

public class NetworkInstaller(Application application, DockerService dockerService, State state)
    : IInstaller<Network>, IHandler<DockerNetwork>
{
    public Application Application => application;

    IEnumerable<InstallerInitializeResult> IInstaller<Network>.Initialize(Network contract, string prefix)
    {
        var baseName = contract.NetworkName ?? prefix + (contract.Name == "" ? "" : $"-{contract.Name}");

        var count = 1;
        var name = baseName;
        while (state.Contracts.OfType<Network>().Any(c => c.Result?.Name == name))
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

    Network IInstaller<Network>.Install(Network contract, ExecutionPlan plan)
    {
        var networkName = contract.NetworkName;
        Debug.Assert(networkName != null, "Network name cannot be null.");

        dockerService.CreateNetwork(networkName).Wait();

        return contract with
        {
            Result = new DockerNetwork(this) { Name = networkName }
        };
    }

    void IHandler<DockerNetwork>.Uninstall(DockerNetwork resource)
    {
        dockerService.RemoveNetwork(resource.Name).Wait();
    }
}