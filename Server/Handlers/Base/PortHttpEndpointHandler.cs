using Frierun.Server.Data;

namespace Frierun.Server.Handlers.Base;

public class PortHttpEndpointHandler : Handler<HttpEndpoint>
{
    public override IEnumerable<ContractList> Initialize(HttpEndpoint contract, ApplicationContext context)
    {
        var portEndpoint = new PortEndpoint(Protocol.Tcp, contract.Port, Container: contract.Container);
        var portEndpointId = new ContractId<PortEndpoint>(context.Name);
        yield return new ContractList
        {
            [context] = contract with
            {
                ResultSsl = false,
                ResultHost = new Argument<string>(plan => plan.GetContract(portEndpointId).ExternalIp),
                ResultPort = new Argument<int>(plan => plan.GetContract(portEndpointId).ExternalPort),
                Handler = this,
                DependsOn =
                [
                    portEndpointId,
                    contract.Container
                ]
            },
            [portEndpointId] = portEndpoint
        };
    }
}