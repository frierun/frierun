using Frierun.Server.Data;

namespace Frierun.Server.Installers;

public class TraefikHttpEndpointInstaller(Application application)
    : IInstaller<HttpEndpoint>, IUninstaller<TraefikHttpEndpoint>
{
    private readonly DockerContainer _container = application.Resources.OfType<DockerContainer>().First();
    private readonly DockerPortEndpoint _port = application.Resources.OfType<DockerPortEndpoint>().First();

    /// <inheritdoc />
    public Application Application => application;
    
    /// <inheritdoc />
    IEnumerable<InstallerInitializeResult> IInstaller<HttpEndpoint>.Initialize(HttpEndpoint contract, string prefix)
    {
        var connectExternalContainer = new ConnectExternalContainer(_container.Name);        
        yield return new InstallerInitializeResult(
            contract with
            {
                DependsOn = contract.DependsOn.Append(connectExternalContainer).Append(contract.DomainId),
                DependencyOf = contract.DependencyOf.Append(contract.ContainerId),
            },
            [connectExternalContainer]
        );
    }

    /// <inheritdoc />
    Resource IInstaller<HttpEndpoint>.Install(HttpEndpoint contract, ExecutionPlan plan)
    {
        var domain = plan.GetResource<ResolvedDomain>(contract.DomainId).Value;
        var subdomain = domain.Split('.')[0];

        var containerContract = plan.GetContract(contract.ContainerId);

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

        plan.RequireApplication(application);
        return new TraefikHttpEndpoint(domain, _port.Port);
    }
}