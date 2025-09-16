using Frierun.Server.Data;

namespace Frierun.Server.Handlers.Docker;

public class PortEndpointHandler(Application application) : Handler<PortEndpoint>(application)
{
    public override IEnumerable<ContractList> Initialize(PortEndpoint contract, ApplicationContext context)
    {
        if (contract.Port == 0)
        {
            yield break;
        }

        if (contract.ExternalPort != 0)
        {
            if (State.Contracts.OfType<PortEndpoint>()
                .Any(endpoint => endpoint.Port == contract.ExternalPort && endpoint.Protocol == contract.Protocol))
            {
                yield break;
            }
        }
        else
        {
            var port = contract.Port;

            while (State.Contracts.OfType<PortEndpoint>()
                   .Any(endpoint => endpoint.Port == port && endpoint.Protocol == contract.Protocol)
                  )
            {
                port += 1000;
                if (port > 65535)
                {
                    yield break;
                }
            }

            contract = contract with { ExternalPort = port };
        }

        yield return new ContractList
        {
            [context] = contract with
            {
                // TODO: fill the correct ip of the host
                ExternalIp = "127.0.0.1",
                Handler = this,
                DependsOn = [contract.Container]
            },
            [contract.Container] = new Container
            {
                Ports =
                [
                    new ContainerPort(
                        InternalPort: contract.Port,
                        ExternalPort: contract.ExternalPort, 
                        Protocol: contract.Protocol
                    )
                ],
                HandlerApplication = Application?.Name,
            }
        };
    }
}