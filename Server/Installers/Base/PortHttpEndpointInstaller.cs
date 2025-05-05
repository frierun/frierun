using Frierun.Server.Data;

namespace Frierun.Server.Installers.Base;

public class PortHttpEndpointInstaller : IInstaller<HttpEndpoint>, IUninstaller<GenericHttpEndpoint>
{
    /// <inheritdoc />
    public Application? Application => null;

    /// <inheritdoc />
    IEnumerable<InstallerInitializeResult> IInstaller<HttpEndpoint>.Initialize(HttpEndpoint contract, string prefix)
    {
        var portEndpoint = new PortEndpoint(
            Protocol.Tcp, contract.Port, ContainerName: contract.ContainerName, DestinationPort: 80
        );
        yield return new InstallerInitializeResult(
            contract with
            {
                DependsOn = contract.DependsOn.Append(portEndpoint),
                DependencyOf = contract.DependencyOf.Append(contract.ContainerId),
            },
            [portEndpoint]
        );
    }

    /// <inheritdoc />
    Resource IInstaller<HttpEndpoint>.Install(HttpEndpoint contract, ExecutionPlan plan)
    {
        var portEndpoint = plan.GetResource<DockerPortEndpoint>(
            new PortEndpoint(Protocol.Tcp, contract.Port, ContainerName: contract.ContainerName, DestinationPort: 80)
        );

        var url = new Uri($"http://{portEndpoint.Ip}:{portEndpoint.Port}");
        return new GenericHttpEndpoint{Url = url};
    }
}