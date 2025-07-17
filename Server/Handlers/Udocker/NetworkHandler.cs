using System.Diagnostics;
using Frierun.Server.Data;
using Network = Frierun.Server.Data.Network;

namespace Frierun.Server.Handlers.Udocker;

public class NetworkHandler(Application application) : Handler<Network>(application)
{
    public override IEnumerable<ContractInitializeResult> Initialize(Network contract, string prefix)
    {
        yield return new ContractInitializeResult(
            contract with
            {
                Handler = this,
                NetworkName = "udocker"
            }
        );
    }

    public override Network Install(Network contract, ExecutionPlan plan)
    {
        var networkName = contract.NetworkName;
        Debug.Assert(networkName == "udocker");
        return contract;
    }
}