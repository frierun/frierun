using System.Diagnostics;
using Frierun.Server.Data;

namespace Frierun.Server.Handlers;

public class TraefikHttpEndpointHandler(Application application)
    : Handler<HttpEndpoint>(application)
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

    public override IEnumerable<ContractInitializeResult> Initialize(HttpEndpoint contract, string prefix)
    {
        yield return new ContractInitializeResult(
            contract with
            {
                Handler = this,
                DependsOn = contract.DependsOn.Append(new Network("")).Append(contract.Domain),
                DependencyOf = contract.DependencyOf.Append(contract.Container),
            }
        );
    }

    public override HttpEndpoint Install(HttpEndpoint contract, ExecutionPlan plan)
    {
        var domainContract = plan.GetContract(contract.Domain);
        Debug.Assert(domainContract.Installed);
        var domain = domainContract.Value;
        var subdomain = domain.Split('.')[0];

        var containerContract = plan.GetContract(contract.Container);
        var network = plan.GetContract(containerContract.Network);
        Debug.Assert(network.Installed);

        _container.AttachNetwork(network.NetworkName);

        string? certResolver = null;
        if (domainContract.IsInternal == false)
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
                NetworkName = network.NetworkName
            }
        };
    }

    public override void Uninstall(HttpEndpoint contract)
    {
        var resource = contract.Result as TraefikHttpEndpoint;
        Debug.Assert(resource != null);
        
        _container.DetachNetwork(resource.NetworkName);
    }
}