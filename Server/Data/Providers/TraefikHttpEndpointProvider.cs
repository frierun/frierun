using Frierun.Server.Services;

namespace Frierun.Server.Data;

public class TraefikHttpEndpointProvider(DockerService dockerService, Application application)
    : IInstaller<HttpEndpoint>, IUninstaller<TraefikHttpEndpoint>
{
    /// <inheritdoc />
    public IEnumerable<ContractDependency> Dependencies(HttpEndpoint contract, ExecutionPlan plan)
    {
        yield return new ContractDependency(
            contract,
            new Container(contract.ContainerName)
        );

        var container = plan.GetContract(contract.ContainerId);
        yield return new ContractDependency(
            new Network(container.NetworkName),
            contract
        );
    }

    /// <inheritdoc />
    public Contract Initialize(HttpEndpoint contract, ExecutionPlan plan)
    {
        var baseName = contract.DomainName ?? $"{plan.Prefix}.localhost";
        var subdomain = baseName.Split('.')[0];
        var domain = baseName.Substring(subdomain.Length + 1);

        var count = 1;
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
    public Resource Install(HttpEndpoint contract, ExecutionPlan plan)
    {
        var domain = contract.DomainName!;
        var subdomain = domain.Split('.')[0];

        var containerContract = plan.GetContract(contract.ContainerId);

        var network = plan.GetResource<DockerNetwork>(containerContract.NetworkId);

        var traefikContainer = application.AllDependencies.OfType<DockerContainer>().FirstOrDefault();
        if (traefikContainer == null)
        {
            throw new Exception("Traefik container not found");
        }

        var traefikPort = application.AllDependencies.OfType<DockerPortEndpoint>().FirstOrDefault();
        if (traefikPort == null)
        {
            throw new Exception("Traefik port not found");
        }

        var resource = new TraefikHttpEndpoint(domain, traefikPort.Port)
        {
            DependsOn =
            [
                application,
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
                        parameters.Labels[$"traefik.http.routers.{subdomain}.rule"] = $"Host(`{domain}`)";
                        parameters.Labels[$"traefik.http.services.{subdomain}.loadbalancer.server.port"] =
                            contract.Port.ToString();
                    }
                ),
            }
        );

        dockerService.AttachNetwork(network.Name, traefikContainer.Name).Wait();
        return resource;
    }

    /// <inheritdoc />
    public void Uninstall(TraefikHttpEndpoint resource)
    {
        var containerGroup = resource.DependsOn.OfType<DockerNetwork>().FirstOrDefault();
        if (containerGroup == null)
        {
            return;
        }

        var traefikContainer = application.AllDependencies.OfType<DockerContainer>().FirstOrDefault();
        if (traefikContainer == null)
        {
            throw new Exception("Traefik container not found");
        }

        dockerService.DetachNetwork(containerGroup.Name, traefikContainer.Name).Wait();
    }
}