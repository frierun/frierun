using Frierun.Server.Data;

namespace Frierun.Server.Handlers.Base;

public class RedisHandler(State state) : Handler<Redis>(state)
{
    public override IEnumerable<ContractList> Initialize(Redis contract, ApplicationContext context)
    {
        var name = "redis" + (string.IsNullOrEmpty(context.Name) ? "" : $"-{context.Name}");
        var containerId = contract.Container ?? new ContractRef<Container>(name);

        var volumeName = contract.Volume?.Name ?? name + "-data";

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
                Network: new ContractId<Network>(Guid.Empty, contract.Network.Name),
                ContainerName: contract.Host,
                Mounts: new Dictionary<string, ContainerMount> { { "/data", new ContainerMount(Volume: new ContractId<Volume>(Guid.Empty, volumeName)) } }
            )
        };
    }
}