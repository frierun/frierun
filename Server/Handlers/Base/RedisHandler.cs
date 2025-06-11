using System.Diagnostics;
using Frierun.Server.Data;

namespace Frierun.Server.Handlers.Base;

public class RedisHandler : Handler<Redis>
{
    public override IEnumerable<ContractInitializeResult> Initialize(Redis contract, string prefix)
    {
        var name = "redis" + (string.IsNullOrEmpty(contract.Name) ? "" : $"-{contract.Name}");
        var container = contract.Container ?? new ContractId<Container>(name);

        var volume = contract.Volume ?? new ContractId<Volume>(name + "-data");

        yield return new ContractInitializeResult(
            contract with
            {
                Handler = this,
                DependsOn = contract.DependsOn.Append(container),
                Container = container
            },
            [
                new Container(
                    Name: container.Name,
                    ImageName: "redis:7",
                    Network: contract.Network,
                    ContainerName: contract.Host
                ),
                new Mount(
                    Path: "/data",
                    Volume: volume,
                    Container: container
                )
            ]
        );
    }

    public override Redis Install(Redis contract, ExecutionPlan plan)
    {
        Debug.Assert(contract.Container != null);
        
        var container = plan.GetContract(contract.Container);
        Debug.Assert(container.Installed);
        Debug.Assert(contract.Host == null || contract.Host == container.ContainerName);

        return contract with
        {
            Host = container.ContainerName
        };
    }
}