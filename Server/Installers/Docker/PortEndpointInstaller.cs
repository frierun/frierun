using Docker.DotNet.Models;
using Frierun.Server.Data;
using Frierun.Server.Installers.Base;

namespace Frierun.Server.Installers.Docker;

public class PortEndpointInstaller(Application application, State state) : IInstaller<PortEndpoint>
{
    public Application Application => application;

    IEnumerable<InstallerInitializeResult> IInstaller<PortEndpoint>.Initialize(PortEndpoint contract, string prefix)
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

        yield return new InstallerInitializeResult(
            contract with
            {
                DestinationPort = port,
                DependencyOf = contract.DependencyOf.Append(contract.ContainerId),
            }
        );
    }

    PortEndpoint IInstaller<PortEndpoint>.Install(PortEndpoint contract, ExecutionPlan plan)
    {
        var containerContract = plan.GetContract(contract.ContainerId);

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
            Result = new DockerPortEndpoint(new EmptyHandler())
            {
                Name = contract.Name,
                Ip = "127.0.0.1",
                Port = externalPort,
                Protocol = contract.Protocol
            }
        };
    }
}