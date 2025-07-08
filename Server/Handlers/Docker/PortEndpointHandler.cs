using Frierun.Server.Data;

namespace Frierun.Server.Handlers.Docker;

public class PortEndpointHandler(Application application) : Handler<PortEndpoint>(application)
{
    public override IEnumerable<ContractInitializeResult> Initialize(PortEndpoint contract, string prefix)
    {
        var port = contract.ExternalPort == 0 ? contract.Port : contract.ExternalPort;

        while (State.Contracts.OfType<PortEndpoint>()
               .Any(endpoint => endpoint.Port == port && endpoint.Protocol == contract.Protocol))
        {
            port += 1000;
            if (port > 65535)
            {
                throw new Exception("No more ports available");
            }
        }

        yield return new ContractInitializeResult(
            contract with
            {
                Handler = this,
                ExternalPort = port,
                DependencyOf = contract.DependencyOf.Append(contract.Container),
            }
        );
    }

    public override PortEndpoint Install(PortEndpoint contract, ExecutionPlan plan)
    {
        // TODO: fill the correct ip of the host
        return contract with
        {
            ExternalIp = "127.0.0.1",
        };
    }
}