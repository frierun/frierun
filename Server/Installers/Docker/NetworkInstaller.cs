using System.Diagnostics;
using Frierun.Server.Data;
using Network = Frierun.Server.Data.Network;

namespace Frierun.Server.Installers.Docker;

public class NetworkInstaller(Application application, DockerService dockerService, State state)
    : IHandler<Network>
{
    public Application Application => application;

    public IEnumerable<InstallerInitializeResult> Initialize(Network contract, string prefix)
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
                Handler = this,
                NetworkName = name
            }
        );
    }

    public Network Install(Network contract, ExecutionPlan plan)
    {
        var networkName = contract.NetworkName;
        Debug.Assert(networkName != null, "Network name cannot be null.");

        dockerService.CreateNetwork(networkName).Wait();

        return contract with
        {
            Result = new DockerNetwork { Name = networkName }
        };
    }

    public void Uninstall(Network contract)
    {
        Debug.Assert(contract.Result != null);
        dockerService.RemoveNetwork(contract.Result.Name).Wait();
    }
}