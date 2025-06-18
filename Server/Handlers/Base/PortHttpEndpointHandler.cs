using Frierun.Server.Data;

namespace Frierun.Server.Handlers.Base;

public class PortHttpEndpointHandler : Handler<HttpEndpoint>
{
    public override IEnumerable<ContractInitializeResult> Initialize(HttpEndpoint contract, string prefix)
    {
        var portEndpoint = CreatePortEndpoint(contract);
        yield return new ContractInitializeResult(
            contract with
            {
                Handler = this,
                DependsOn = contract.DependsOn.Append(portEndpoint),
                DependencyOf = contract.DependencyOf.Append(contract.Container),
            },
            [portEndpoint]
        );
    }

    public override HttpEndpoint Install(HttpEndpoint contract, ExecutionPlan plan)
    {
        var portEndpoint = plan.GetContract((ContractId<PortEndpoint>)CreatePortEndpoint(contract).Id);

        var url = new Uri($"http://{portEndpoint.ExternalIp}:{portEndpoint.ExternalPort}");

        return contract with
        {
            ResultSsl = false,
            ResultHost = portEndpoint.ExternalIp,
            ResultPort = portEndpoint.ExternalPort,
        };
    }
    
    private static PortEndpoint CreatePortEndpoint(HttpEndpoint contract)
    {
        return new PortEndpoint(
            Protocol.Tcp, contract.Port, Container: contract.Container, ExternalPort: 80
        );
    }
}