using Docker.DotNet.Models;
using Frierun.Server.Data;

namespace Frierun.Server.Installers.Docker;

public class PortEndpointInstaller(State state) : IInstaller<PortEndpoint>, IUninstaller<DockerPortEndpoint>
{
    /// <inheritdoc />
    public Application? Application => null;

    /// <inheritdoc />
    IEnumerable<InstallerInitializeResult> IInstaller<PortEndpoint>.Initialize(PortEndpoint contract, string prefix)
    {
        int port = contract.DestinationPort == 0 ? contract.Port : contract.DestinationPort;

        while (state.Resources.OfType<DockerPortEndpoint>()
               .Any(endpoint => endpoint.Port == port && endpoint.Protocol == contract.Protocol))
        {
            port += 1000;
            if (port > 65535)
            {
                throw new Exception("No more ports available");
            }
        }

        yield return new InstallerInitializeResult(
            contract with
            {
                DestinationPort = port,
                DependencyOf = contract.DependencyOf.Append(contract.ContainerId),
            }
        );
    }

    /// <inheritdoc />
    Resource IInstaller<PortEndpoint>.Install(PortEndpoint contract, ExecutionPlan plan)
    {
        var containerContract = plan.GetContract(contract.ContainerId);

        if (containerContract == null)
        {
            throw new Exception("Container not found");
        }

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
        return new DockerPortEndpoint
        {
            Name = contract.Name,
            Ip = "127.0.0.1", 
            Port = externalPort,
            Protocol = contract.Protocol
        };
    }
}