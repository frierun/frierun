using Frierun.Server.Data;

namespace Frierun.Server.Handlers.Udocker;

public class PortEndpointHandler(Application application) : Handler<PortEndpoint>(application)
{
    private readonly SshConnection _connection = application.Contracts.OfType<SshConnection>().Single();
    
    public override IEnumerable<ContractInitializeResult> Initialize(PortEndpoint contract, string prefix)
    {
        if (contract.Port == 0)
        {
            yield break;
        }
        
        if (contract.ExternalPort is > 0 and < 1024)
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

            while (port < 1024
                   || State.Contracts.OfType<PortEndpoint>()
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

        yield return new ContractInitializeResult(
            contract with
            {
                Handler = this,
                DependencyOf = contract.DependencyOf.Append(contract.Container),
            }
        );
    }

    public override PortEndpoint Install(PortEndpoint contract, ExecutionPlan plan)
    {
        return contract with
        {
            ExternalIp = _connection.Host,
        };
    }
}