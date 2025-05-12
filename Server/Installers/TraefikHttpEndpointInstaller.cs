using Frierun.Server.Data;
using Frierun.Server.Installers.Base;

namespace Frierun.Server.Installers;

public class TraefikHttpEndpointInstaller(Application application, State state)
    : IInstaller<HttpEndpoint>, IHandler<TraefikHttpEndpoint>
{
    private readonly DockerContainer _container = application.Resources.OfType<DockerContainer>().First();

    private readonly int _webPort = application.Resources.OfType<DockerPortEndpoint>()
        .FirstOrDefault(endpoint => endpoint.Name == "Web")?.Port ?? 0;

    private readonly int _webSecurePort = application.Resources.OfType<DockerPortEndpoint>()
        .FirstOrDefault(endpoint => endpoint.Name == "WebSecure")?.Port ?? 0;

    public Application Application => application;

    IEnumerable<InstallerInitializeResult> IInstaller<HttpEndpoint>.Initialize(HttpEndpoint contract, string prefix)
    {
        yield return new InstallerInitializeResult(
            contract with
            {
                DependsOn = contract.DependsOn.Append(new Network("")).Append(contract.DomainId),
                DependencyOf = contract.DependencyOf.Append(contract.ContainerId),
            }
        );
    }

    Resource IInstaller<HttpEndpoint>.Install(HttpEndpoint contract, ExecutionPlan plan)
    {
        var domainResource = plan.GetResource<ResolvedDomain>(contract.DomainId);
        var domain = domainResource.Value;
        var subdomain = domain.Split('.')[0];

        var containerContract = plan.GetContract(contract.ContainerId);
        var network = plan.GetResource<DockerNetwork>(containerContract.NetworkId);

        _container.AttachNetwork(network.Name);

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

        return new TraefikHttpEndpoint(this)
        {
            Url = url,
            NetworkName = network.Name
        };
    }

    void IHandler<TraefikHttpEndpoint>.Uninstall(TraefikHttpEndpoint resource)
    {
        _container.DetachNetwork(resource.NetworkName);
    }
}