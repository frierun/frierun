using Frierun.Server.Data;

namespace Frierun.Server.Handlers.Udocker;

public class PortEndpointHandler(State state, Application application) : Handler<PortEndpoint>(state, application)
{
    private readonly SshConnection _connection = state.GetContract<SshConnection>(application);

    public override IEnumerable<ContractList> Initialize(PortEndpoint contract, ApplicationContext context)
    {
        if (contract.Port == 0)
        {
            yield break;
        }

        if (contract.ExternalPort is > 0 and < 1024)
        {
            yield break;
        }

        if (contract.Protocol != Protocol.Tcp)
        {
            yield break;
        }

        if (contract.ExternalPort != 0)
        {
            if (State.GetContracts<PortEndpoint>()
                .Any(endpoint => endpoint.Port == contract.ExternalPort && endpoint.Protocol == contract.Protocol))
            {
                yield break;
            }
        }
        else
        {
            var port = contract.Port;

            while (port < 1024
                   || State.GetContracts<PortEndpoint>()
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
                ExternalIp = _connection.Host,
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