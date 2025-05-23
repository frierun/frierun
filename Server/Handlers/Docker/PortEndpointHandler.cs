using Docker.DotNet.Models;
using Frierun.Server.Data;

namespace Frierun.Server.Handlers.Docker;

public class PortEndpointHandler(Application application, State state) : IHandler<PortEndpoint>
{
    public Application Application => application;

    public IEnumerable<ContractInitializeResult> Initialize(PortEndpoint contract, string prefix)
    {
        int port = contract.DestinationPort == 0 ? contract.Port : contract.DestinationPort;

        while (state.Contracts.OfType<PortEndpoint>()
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
                DestinationPort = port,
                DependencyOf = contract.DependencyOf.Append(contract.Container),
            }
        );
    }

    public PortEndpoint Install(PortEndpoint contract, ExecutionPlan plan)
    {
        var containerContract = plan.GetContract(contract.Container);

        var externalPort = contract.DestinationPort;
        var internalPort = contract.Port;

        plan.UpdateContract(
            containerContract with
            {
                Configure = containerContract.Configure.Append(
                    parameters =>
                    {
                        parameters.HostConfig.PortBindings[$"{internalPort}/{contract.Protocol.ToString().ToLower()}"] =
                            new List<PortBinding>
                            {
                                new()
                                {
                                    HostPort = externalPort.ToString()
                                }
                            };
                    }
                ),
            }
        );

        // TODO: fill the correct ip of the host
        return contract with
        {
            Result = new DockerPortEndpoint
            {
                Name = contract.Name,
                Ip = "127.0.0.1",
                Port = externalPort,
                Protocol = contract.Protocol
            }
        };
    }
}