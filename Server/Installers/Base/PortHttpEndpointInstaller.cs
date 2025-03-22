using Frierun.Server.Data;

namespace Frierun.Server.Installers.Base;

public class PortHttpEndpointInstaller : IInstaller<HttpEndpoint>, IUninstaller<GenericHttpEndpoint>
{
    /// <inheritdoc />
    IEnumerable<InstallerInitializeResult> IInstaller<HttpEndpoint>.Initialize(HttpEndpoint contract, string prefix)
    {
        var portEndpoint = new PortEndpoint(Protocol.Tcp, contract.Port, contract.ContainerName, 80);
        yield return new InstallerInitializeResult(
            contract,
            [contract.ContainerId],
            [portEndpoint]
        );
    }

    /// <inheritdoc />
    IEnumerable<ContractDependency> IInstaller<HttpEndpoint>.GetDependencies(HttpEndpoint contract, ExecutionPlan plan)
    {
        yield return new ContractDependency(contract, contract.ContainerId);
        yield return new ContractDependency(
            new PortEndpoint(Protocol.Tcp, contract.Port, contract.ContainerName, 80),
            contract
        );
    }

    /// <inheritdoc />
    Resource IInstaller<HttpEndpoint>.Install(HttpEndpoint contract, ExecutionPlan plan)
    {
        var portEndpoint = plan.GetResource<DockerPortEndpoint>(
            new PortEndpoint(Protocol.Tcp, contract.Port, contract.ContainerName, 80)
        );

        var url = new Uri($"http://{portEndpoint.Ip}:{portEndpoint.Port}");
        return new GenericHttpEndpoint(url);
    }
}