using Docker.DotNet.Models;

namespace Frierun.Server.Data;

public class PortHttpEndpointProvider : Provider<HttpEndpoint, HttpEndpointContract>
{
    /// <inheritdoc />
    protected override IEnumerable<ContractDependency> Dependencies(HttpEndpointContract contract, ExecutionPlan plan)
    {
        yield return new ContractDependency(
            contract,
            new ContainerContract(contract.ContainerName)
        );
    }

    /// <inheritdoc />
    protected override HttpEndpoint Install(HttpEndpointContract contract, ExecutionPlan plan)
    {
        var containerContract = plan.GetContract<ContainerContract>(contract.ContainerId);

        if (containerContract == null)
        {
            throw new Exception("Container not found");
        }

        // TODO: fill the correct external port
        var externalPort = contract.Port;
        var internalPort = contract.Port;

        // TODO: fill the correct ip of the host
        var endpoint = new PortHttpEndpoint("127.0.0.1", externalPort);

        plan.UpdateContract(
            containerContract with
            {
                Configure = containerContract.Configure.Append(
                    parameters =>
                    {
                        parameters.HostConfig.PortBindings[$"{internalPort}/tcp"] =
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