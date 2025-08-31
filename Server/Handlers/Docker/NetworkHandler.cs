using System.Diagnostics;
using Frierun.Server.Data;
using Network = Frierun.Server.Data.Network;

namespace Frierun.Server.Handlers.Docker;

public class NetworkHandler(Application application, DockerService dockerService)
    : Handler<Network>(application)
{
    public override IEnumerable<ContractList> Initialize(Network contract, ApplicationContext context)
    {
        yield return
        [
            contract with
            {
                Handler = this,
                NetworkName = contract.NetworkName ?? FindUniqueName(
                    context.Prefix + (context.Name == "" ? "" : $"-{context.Name}"),
                    c => c.NetworkName
                )
            }
        ];
    }

    public override Network Install(Network contract, ExecutionPlan plan)
    {
        var networkName = contract.NetworkName;
        Debug.Assert(networkName != null, "Network name cannot be null.");

        dockerService.CreateNetwork(networkName).Wait();

        return contract;
    }

    public override void Uninstall(Network contract)
    {
        Debug.Assert(contract.Installed);
        dockerService.RemoveNetwork(contract.NetworkName).Wait();
    }
}