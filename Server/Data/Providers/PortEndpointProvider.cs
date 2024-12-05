using Docker.DotNet.Models;

namespace Frierun.Server.Data;

public class PortEndpointProvider : IInstaller<PortEndpoint>, IUninstaller<DockerPortEndpoint>
{
    /// <inheritdoc />
    public IEnumerable<ContractDependency> Dependencies(PortEndpoint contract, ExecutionPlan plan)
    {
        yield return new ContractDependency(
            contract,
            new Container(contract.ContainerName)
        );
    }

    /// <inheritdoc />
    public Contract Initialize(PortEndpoint contract, ExecutionPlan plan)
    {
        int port = contract.DestinationPort == 0 ? contract.Port : contract.DestinationPort;

        while (plan.State.Resources.OfType<DockerPortEndpoint>()
               .Any(endpoint => endpoint.Port == port && endpoint.Protocol == contract.Protocol))
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
    public Resource Install(PortEndpoint contract, ExecutionPlan plan)
    {
        var containerContract = plan.GetContract(contract.ContainerId);

        if (containerContract == null)
        {
            throw new Exception("Container not found");
        }

        var externalPort = contract.DestinationPort;
        var internalPort = contract.Port;

        // TODO: fill the correct ip of the host
        var endpoint = new DockerPortEndpoint("127.0.0.1", externalPort, contract.Protocol);

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
                DependsOn = containerContract.DependsOn.Append(endpoint)
            }
        );

        return endpoint;
    }

    /// <inheritdoc />
    public void Uninstall(DockerPortEndpoint resource)
    {
    }
}