using System.Diagnostics;
using Frierun.Server.Data;
using Network = Frierun.Server.Data.Network;

namespace Frierun.Server.Handlers.Docker;

public class NetworkHandler(Application application, DockerService dockerService, State state)
    : IHandler<Network>
{
    public Application Application => application;

    public IEnumerable<ContractInitializeResult> Initialize(Network contract, string prefix)
    {
        var baseName = contract.NetworkName ?? prefix + (contract.Name == "" ? "" : $"-{contract.Name}");

        var count = 1;
        var name = baseName;
        while (state.Contracts.OfType<Network>().Any(c => c.NetworkName == name))
        {
            count++;
            name = $"{baseName}{count}";
        }

        yield return new ContractInitializeResult(
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

        return contract;
    }

    public void Uninstall(Network contract)
    {
        Debug.Assert(contract.Installed);
        dockerService.RemoveNetwork(contract.NetworkName).Wait();
    }
}