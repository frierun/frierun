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
        ?.ExternalPort ?? 0;

    private readonly int _webSecurePort = application.Contracts
        .OfType<PortEndpoint>()
        .FirstOrDefault(endpoint => endpoint.Name == "WebSecure")
        ?.ExternalPort ?? 0;

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

        var container = plan.GetContract(contract.Container);
        var network = plan.GetContract(container.Network);
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
            container with
            {
                Configure = container.Configure.Append(
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

        return contract with
        {
            ResultSsl = certResolver != null,
            ResultHost = domain,
            ResultPort = certResolver != null ? _webSecurePort : _webPort,
            NetworkName = network.NetworkName,
        };
    }

    public override void Uninstall(HttpEndpoint contract)
    {
        Debug.Assert(contract.Installed);
        Debug.Assert(contract.NetworkName != null);
        
        _container.DetachNetwork(contract.NetworkName);
    }
}