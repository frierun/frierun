using Frierun.Server.Data;

namespace Frierun.Server.Installers;

public class TraefikHttpEndpointInstaller(Application application)
    : IInstaller<HttpEndpoint>
{
    private readonly DockerContainer _container = application.Resources.OfType<DockerContainer>().First();

    private readonly int _webPort = application.Resources.OfType<DockerPortEndpoint>()
        .FirstOrDefault(endpoint => endpoint.Name == "Web")?.Port ?? 0;

    private readonly int _webSecurePort = application.Resources.OfType<DockerPortEndpoint>()
        .FirstOrDefault(endpoint => endpoint.Name == "WebSecure")?.Port ?? 0;

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
        var domainResource = plan.GetResource<ResolvedDomain>(contract.DomainId);
        var domain = domainResource.Value;
        var subdomain = domain.Split('.')[0];

        var containerContract = plan.GetContract(contract.ContainerId);

        string? certResolver = null;
        if (!domainResource.IsInternal)
        {
            if (_webPort == 80)
            {
                certResolver = "httpchallenge";
            }
            else if (_webSecurePort == 443)
            {
                certResolver = "tlschallenge";
            }
        }

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

                        if (certResolver != null)
                        {
                            parameters.Labels[$"traefik.http.routers.{subdomain}.tls"] = "true";
                            parameters.Labels[$"traefik.http.routers.{subdomain}.tls.certresolver"] = certResolver;
                        }
                    }
                ),
            }
        );

        var url = certResolver == null
            ? new Uri($"http://{domain}:{_webPort}")
            : new Uri($"https://{domain}:{_webSecurePort}");

        return new GenericHttpEndpoint
        {
            Url = url
        };
    }
}