using Frierun.Server.Services;

namespace Frierun.Server.Data;

public class TraefikHttpEndpointProvider(DockerService dockerService, Application application)
    : Provider<HttpEndpoint, HttpEndpointContract>
{
    /// <inheritdoc />
    protected override IEnumerable<ContractDependency> Dependencies(HttpEndpointContract contract, ExecutionPlan plan)
    {
        yield return new ContractDependency(
            contract,
            new ContainerContract(contract.ContainerName)
        );
        
        var container = plan.GetContract<ContainerContract>(contract.ContainerId);
        yield return new ContractDependency(
            new NetworkContract(container.NetworkName),
            contract
        );
    }
    
    /// <inheritdoc />
    protected override HttpEndpointContract Initialize(HttpEndpointContract contract, ExecutionPlan plan)
    {
        var baseName = contract.DomainName ?? $"{plan.Prefix}.localhost";
        var subdomain = baseName.Split('.')[0];
        var domain = baseName.Substring(subdomain.Length + 1);
        
        var count = 0;
        var name = baseName;
        while (plan.State.Resources.OfType<TraefikHttpEndpoint>().Any(c => c.Domain == name))
        {
            count++;
            name = $"{subdomain}{count}.{domain}";
        }

        return contract.DomainName == name
            ? contract
            : contract with
            {
                DomainName = name
            };
    }    
    
    /// <inheritdoc />
    protected override HttpEndpoint Install(HttpEndpointContract contract, ExecutionPlan plan)
    {
        var domain = contract.DomainName!;
        var subdomain = domain.Split('.')[0];

        var containerContract = plan.GetContract<ContainerContract>(contract.ContainerId);

        if (containerContract == null)
        {
            throw new Exception("Container not found");
        }

        var network = plan.GetResource<Network>(containerContract.NetworkId);

        var traefikContainer = application.AllDependencies.OfType<Container>().FirstOrDefault();
        if (traefikContainer == null)
        {
            throw new Exception("Traefik container not found");
        }

        var traefikPort = application.AllDependencies.OfType<PortHttpEndpoint>().FirstOrDefault();
        if (traefikPort == null)
        {
            throw new Exception("Traefik port not found");
        }

        var resource = new TraefikHttpEndpoint(domain, traefikPort.Port)
        {
            DependsOn =
            [
                traefikContainer,
                network
            ]
        };

        plan.UpdateContract(
            containerContract with
            {
                Configure = containerContract.Configure.Append(
                    parameters =>
                    {
                        parameters.Labels["traefik.enable"] = "true";
                        parameters.Labels["traefik.http.routers." + subdomain + ".rule"] = "Host(`" + domain + "`)";
                        parameters.Labels["traefik.http.services." + subdomain + ".loadbalancer.server.port"] =
                            contract.Port.ToString();
                    }
                ),
                DependsOn = containerContract.DependsOn.Append(resource)
            }
        );

        dockerService.AttachNetwork(network.Name, traefikContainer.Name).Wait();
        return resource;
    }
    
    /// <inheritdoc />
    protected override void Uninstall(HttpEndpoint resource)
    {
        var containerGroup = resource.DependsOn.OfType<Network>().FirstOrDefault();
        if (containerGroup == null)
        {
            return;
        }

        var traefikContainer = application.AllDependencies.OfType<Container>().FirstOrDefault();
        if (traefikContainer == null)
        {
            throw new Exception("Traefik container not found");
        }

        dockerService.DetachNetwork(containerGroup.Name, traefikContainer.Name).Wait();
    }
}