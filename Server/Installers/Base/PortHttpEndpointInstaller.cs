using Frierun.Server.Data;

namespace Frierun.Server.Installers.Base;

public class PortHttpEndpointInstaller : IInstaller<HttpEndpoint>, IUninstaller<GenericHttpEndpoint>
{
    /// <inheritdoc />
    public IEnumerable<ContractDependency> Dependencies(HttpEndpoint contract, ExecutionPlan plan)
    {
        yield return new ContractDependency(
            contract,
            new Container(contract.ContainerName)
        );

        yield return new ContractDependency(
            new PortEndpoint(Protocol.Tcp, contract.Port, contract.ContainerName, 80),
            contract
        );
    }

    /// <inheritdoc />
    public Resource? Install(HttpEndpoint contract, ExecutionPlan plan)
    {
        var portEndpoint = plan.GetResource<DockerPortEndpoint>(
            new PortEndpoint(Protocol.Tcp, contract.Port, contract.ContainerName, 80).Id
        );

        var url = new Uri($"http://{portEndpoint.Ip}:{portEndpoint.Port}");
        
        var endpoint = new GenericHttpEndpoint(url);

        return endpoint;
    }
}