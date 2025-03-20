using Frierun.Server.Data;

namespace Frierun.Server.Installers;

public class TraefikHttpEndpointInstaller(Application application)
    : IInstaller<HttpEndpoint>, IUninstaller<TraefikHttpEndpoint>
{
    private readonly DockerContainer _container = application.DependsOn.OfType<DockerContainer>().First();
    private readonly DockerPortEndpoint _port = application.DependsOn.OfType<DockerPortEndpoint>().First();
    
    /// <inheritdoc />
    IEnumerable<InstallerInitializeResult> IInstaller<HttpEndpoint>.Initialize(HttpEndpoint contract, string prefix, State state)
    {
        var baseName = contract.DomainName ?? $"{prefix}.localhost";
        var subdomain = baseName.Split('.')[0];
        var domain = baseName.Substring(subdomain.Length + 1);

        var count = 1;
        var name = baseName;
        while (state.Resources.OfType<TraefikHttpEndpoint>().Any(c => c.Domain == name))
        {
            count++;
            name = $"{subdomain}{count}.{domain}";
        }

        yield return new InstallerInitializeResult(
            contract with { DomainName = name },
            [contract.ContainerId],
            [new ConnectExternalContainer(_container.Name)]
        );
    }

    /// <inheritdoc />
    IEnumerable<ContractDependency> IInstaller<HttpEndpoint>.GetDependencies(HttpEndpoint contract, ExecutionPlan plan)
    {
        yield return new ContractDependency(contract.Id, contract.ContainerId);
        yield return new ContractDependency(new ConnectExternalContainer(_container.Name).Id, contract.Id);
    }

    /// <inheritdoc />
    Resource IInstaller<HttpEndpoint>.Install(HttpEndpoint contract, ExecutionPlan plan)
    {
        var domain = contract.DomainName!;
        var subdomain = domain.Split('.')[0];

        var containerContract = plan.GetContract(contract.ContainerId);

        var attachNetworkContract = new ConnectExternalContainer(_container.Name);
        var attachedNetwork = plan.GetResource<DockerAttachedNetwork>(attachNetworkContract.Id);
        
        var resource = new TraefikHttpEndpoint(domain, _port.Port)
        {
            DependsOn =
            [
                application,
                attachedNetwork
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

        return resource;
    }
}