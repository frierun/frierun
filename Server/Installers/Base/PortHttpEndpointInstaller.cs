using Frierun.Server.Data;

namespace Frierun.Server.Installers.Base;

public class PortHttpEndpointInstaller : IInstaller<HttpEndpoint>, IUninstaller<GenericHttpEndpoint>
{
    /// <inheritdoc />
    InstallerInitializeResult IInstaller<HttpEndpoint>.Initialize(HttpEndpoint contract, string prefix, State state)
    {
        var portEndpoint = new PortEndpoint(Protocol.Tcp, contract.Port, contract.ContainerName, 80);
        return new InstallerInitializeResult(
            contract,
            [contract.ContainerId],
            [portEndpoint]
        );
    }

    /// <inheritdoc />
    IEnumerable<ContractDependency> IInstaller<HttpEndpoint>.GetDependencies(HttpEndpoint contract, ExecutionPlan plan)
    {
        var portEndpoint = new PortEndpoint(Protocol.Tcp, contract.Port, contract.ContainerName, 80);

        yield return new ContractDependency(contract.Id, contract.ContainerId);
        yield return new ContractDependency(portEndpoint.Id, contract.Id);
    }

    /// <inheritdoc />
    Resource IInstaller<HttpEndpoint>.Install(HttpEndpoint contract, ExecutionPlan plan)
    {
        var portEndpoint = plan.GetResource<DockerPortEndpoint>(
            new PortEndpoint(Protocol.Tcp, contract.Port, contract.ContainerName, 80).Id
        );

        var url = new Uri($"http://{portEndpoint.Ip}:{portEndpoint.Port}");
        return new GenericHttpEndpoint(url);
    }
}