using Docker.DotNet.Models;

namespace Frierun.Server.Data;

public class PortEndpointProvider : Provider<PortEndpoint, PortEndpointContract>
{
    /// <inheritdoc />
    protected override IEnumerable<ContractDependency> Dependencies(PortEndpointContract contract, ExecutionPlan plan)
    {
        yield return new ContractDependency(
            contract,
            new ContainerContract(contract.ContainerName)
        );
    }

    /// <inheritdoc />
    protected override PortEndpointContract Initialize(PortEndpointContract contract, ExecutionPlan plan)
    {
        int port = contract.DestinationPort == 0 ? contract.Port : contract.DestinationPort;

        while (plan.State.Resources.OfType<PortEndpoint>()
               .Any(endpoint => endpoint.Port == port && endpoint.PortType == contract.PortType))
        {
            port += 1000;
            if (port > 65535)
            {
                throw new Exception("No more ports available");
            }
        }

        return contract.DestinationPort == port
            ? contract
            : contract with
            {
                DestinationPort = port
            };
    }

    /// <inheritdoc />
    protected override PortEndpoint Install(PortEndpointContract contract, ExecutionPlan plan)
    {
        var containerContract = plan.GetContract<ContainerContract>(contract.ContainerId);

        if (containerContract == null)
        {
            throw new Exception("Container not found");
        }

        var externalPort = contract.DestinationPort;
        var internalPort = contract.Port;

        // TODO: fill the correct ip of the host
        var endpoint = new PortEndpoint("127.0.0.1", externalPort, contract.PortType);

        plan.UpdateContract(
            containerContract with
            {
                Configure = containerContract.Configure.Append(
                    parameters =>
                    {
                        parameters.HostConfig.PortBindings[$"{internalPort}/{contract.PortType}"] =
                            new List<PortBinding>
                            {
                                new()
                                {
                                    HostPort = externalPort.ToString()
                                }
                            };
                    }
                ),
                DependsOn = containerContract.DependsOn.Append(endpoint)
            }
        );

        return endpoint;
    }
}