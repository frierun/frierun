using System.Diagnostics;
using Frierun.Server.Data;

namespace Frierun.Server.Handlers;

public class TraefikHttpEndpointHandler(Application application)
    : IHandler<HttpEndpoint>
{
    private readonly Container _container = application.Contracts.OfType<Container>().First();    

    private readonly int _webPort = application.Contracts
        .OfType<PortEndpoint>()
        .FirstOrDefault(endpoint => endpoint.Name == "Web")
        ?.Result
        ?.Port ?? 0;

    private readonly int _webSecurePort = application.Contracts
        .OfType<PortEndpoint>()
        .FirstOrDefault(endpoint => endpoint.Name == "WebSecure")
        ?.Result
        ?.Port ?? 0;

    public Application Application => application;

    public IEnumerable<ContractInitializeResult> Initialize(HttpEndpoint contract, string prefix)
    {
        yield return new ContractInitializeResult(
            contract with
            {
                Handler = this,
                DependsOn = contract.DependsOn.Append(new Network("")).Append(contract.DomainId),
                DependencyOf = contract.DependencyOf.Append(contract.ContainerId),
            }
        );
    }

    public HttpEndpoint Install(HttpEndpoint contract, ExecutionPlan plan)
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

        return contract with
        {
            Result = new TraefikHttpEndpoint
            {
                Url = url,
                NetworkName = network.Name
            }
        };
    }

    void IHandler<HttpEndpoint>.Uninstall(HttpEndpoint contract)
    {
        var resource = contract.Result as TraefikHttpEndpoint;
        Debug.Assert(resource != null);
        
        _container.DetachNetwork(resource.NetworkName);
    }
}