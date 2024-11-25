using System.Text;

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

        yield return new ContractDependency(
            new PortEndpointContract(PortType.Tcp, contract.Port, contract.ContainerName, 80),
            contract
        );
    }

    /// <inheritdoc />
    protected override HttpEndpoint Install(HttpEndpointContract contract, ExecutionPlan plan)
    {
        var portEndpoint = plan.GetResource<PortEndpoint>(
            new PortEndpointContract(PortType.Tcp, contract.Port, contract.ContainerName, 80).Id
        );
        var containerContract = plan.GetContract<ContainerContract>(contract.ContainerId);

        var endpointUrl = new StringBuilder();
        endpointUrl.Append("http://");
        endpointUrl.Append(portEndpoint.Ip);
        if (portEndpoint.Port != 80)
        {
            endpointUrl.Append(":");
            endpointUrl.Append(portEndpoint.Port);
        }
        
        var endpoint = new HttpEndpoint(endpointUrl.ToString());

        plan.UpdateContract(
            containerContract with
            {
                DependsOn = containerContract.DependsOn.Append(endpoint)
            }
        );

        return endpoint;
    }
}