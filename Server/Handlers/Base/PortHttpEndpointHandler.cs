using Frierun.Server.Data;

namespace Frierun.Server.Handlers.Base;

public class PortHttpEndpointHandler : Handler<HttpEndpoint>
{
    public override IEnumerable<ContractInitializeResult> Initialize(HttpEndpoint contract, ApplicationContext context)
    {
        var portEndpoint = CreatePortEndpoint(contract);
        var portEndpointId = portEndpoint.Id;
        yield return new ContractInitializeResult(
            contract with
            {
                ResultSsl = false,
                ResultHost = new Argument<string>(plan => plan.GetContract<PortEndpoint>(portEndpointId).ExternalIp),
                ResultPort = new Argument<int>(plan => plan.GetContract<PortEndpoint>(portEndpointId).ExternalPort),
                Handler = this,
                DependsOn = [
                    portEndpointId,
                    contract.Container
                ],
            },
            [
                portEndpoint
            ]
        );
    }

    private static PortEndpoint CreatePortEndpoint(HttpEndpoint contract)
    {
        return new PortEndpoint(
            Protocol.Tcp, contract.Port, Container: contract.Container
        );
    }
}