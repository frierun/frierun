using Frierun.Server.Data;

namespace Frierun.Server.Handlers.Base;

public class PortHttpEndpointHandler : IHandler<HttpEndpoint>
{
    public IEnumerable<ContractInitializeResult> Initialize(HttpEndpoint contract, string prefix)
    {
        var portEndpoint = new PortEndpoint(
            Protocol.Tcp, contract.Port, Container: contract.Container, DestinationPort: 80
        );
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

    public HttpEndpoint Install(HttpEndpoint contract, ExecutionPlan plan)
    {
        var portEndpoint = plan.GetResource<DockerPortEndpoint>(
            new PortEndpoint(Protocol.Tcp, contract.Port, Container: contract.Container, DestinationPort: 80)
        );

        var url = new Uri($"http://{portEndpoint.Ip}:{portEndpoint.Port}");

        return contract with
        {
            Result = new GenericHttpEndpoint { Url = url }
        };
    }
}