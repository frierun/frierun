using Frierun.Server.Data;

namespace Frierun.Server.Handlers.Base;

public class RedisHandler : Handler<Redis>
{
    public override IEnumerable<ContractInitializeResult> Initialize(Redis contract, string prefix)
    {
        var name = "redis" + (string.IsNullOrEmpty(contract.Name) ? "" : $"-{contract.Name}");
        var container = contract.Container ?? new ContractId<Container>(name);

        var volume = contract.Volume ?? new ContractId<Volume>(name + "-data");

        if (contract.Host.Empty)
        {
            contract = contract with { Host = new Argument<string>(plan => plan.GetContract(container).ContainerName) };
        }

        yield return new ContractInitializeResult(
            contract with
            {
                Handler = this,
                DependsOn = [container],
                Container = container,
            },
            [
                new Container(
                    Name: container.Name,
                    ImageName: "redis:7",
                    Network: contract.Network,
                    ContainerName: contract.Host,
                    Mounts: new Dictionary<string, ContainerMount>() { { "/data", new ContainerMount(Volume: volume) } }
                )
            ]
        );
    }
}