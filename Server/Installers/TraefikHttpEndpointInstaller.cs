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

    /// <inheritdoc />
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

    /// <inheritdoc />
    Resource IInstaller<HttpEndpoint>.Install(HttpEndpoint contract, ExecutionPlan plan)
    {
        var domainResource = plan.GetResource<ResolvedDomain>(contract.DomainId);
        var domain = domainResource.Value;
        var subdomain = domain.Split('.')[0];

        var containerContract = plan.GetContract(contract.ContainerId);
        var network = plan.GetResource<DockerNetwork>(containerContract.NetworkId);
        
        if (CountSameResources(network.Name, plan) == 0)
        {
            _container.AttachNetwork(network.Name);
        }

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
    
    /// <summary>
    /// Counts the number of resources with the same network name and handler
    /// </summary>
    private int CountSameResources(string networkName, ExecutionPlan? plan = null)
    {
        return state.Resources
            .Concat(plan?.Resources.Values ?? Array.Empty<Resource>())
            .OfType<TraefikHttpEndpoint>()
            .Count(
                resource => !resource.Uninstalled && resource.NetworkName == networkName && resource.Handler == this
            );
    }

    /// <inheritdoc />
    void IHandler<TraefikHttpEndpoint>.Uninstall(TraefikHttpEndpoint resource)
    {
        if (CountSameResources(resource.NetworkName) <= 1)
        {
            _container.DetachNetwork(resource.NetworkName);
        }
    }
}