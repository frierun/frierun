using Frierun.Server.Data;

namespace Frierun.Server.Handlers.Base;

public class RedisHandler : Handler<Redis>
{
    public override IEnumerable<ContractList> Initialize(Redis contract, ApplicationContext context)
    {
        var name = "redis" + (string.IsNullOrEmpty(context.Name) ? "" : $"-{context.Name}");
        var containerId = contract.Container ?? new ContractId<Container>(name);

        var volume = contract.Volume ?? new ContractId<Volume>(name + "-data");

        if (contract.Host.Empty)
        {
            contract = contract with { Host = new Argument<string>(plan => plan.GetContract(containerId).ContainerName) };
        }

        yield return new ContractList
        {
            [context] = contract with
            {
                Handler = this,
                DependsOn = [containerId],
                Container = containerId,
            },
            [containerId] = new Container(
                ImageName: "redis:7",
                Network: contract.Network,
                ContainerName: contract.Host,
                Mounts: new Dictionary<string, ContainerMount> { { "/data", new ContainerMount(Volume: volume) } }
            )
        };
    }
}