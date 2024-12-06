using System.Text;

namespace Frierun.Server.Data;

public class PortHttpEndpointProvider : IInstaller<HttpEndpoint>, IUninstaller<GenericHttpEndpoint>
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
        var containerContract = plan.GetContract<Container>(contract.ContainerId);

        var url = new Uri($"http://{portEndpoint.Ip}:{portEndpoint.Port}");
        
        var endpoint = new GenericHttpEndpoint(url);

        plan.UpdateContract(
            containerContract with
            {
                DependsOn = containerContract.DependsOn.Append(endpoint)
            }
        );

        return endpoint;
    }

    /// <inheritdoc />
    public void Uninstall(GenericHttpEndpoint resource)
    {
    }
}