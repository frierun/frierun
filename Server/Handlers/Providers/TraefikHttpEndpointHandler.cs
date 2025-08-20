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
        var traefikRouterName = contract.TraefikRouterName
                                ?? FindUniqueName(
                                    prefix + (contract.Name == "" ? "" : $"-{contract.Name}"),
                                    c => c.TraefikRouterName
                                );

        yield return new ContractInitializeResult(
            contract with
            {
                TraefikRouterName = traefikRouterName,
                ResultSsl = new Argument<bool?>(plan =>
                    GetCertificateResolver(plan.GetContract(contract.Domain)) != null
                ),
                ResultHost = new Argument<string>(plan => plan.GetContract(contract.Domain).Value),
                ResultPort = new Argument<int>(plan =>
                    GetCertificateResolver(plan.GetContract(contract.Domain)) == null ? _webPort : _webSecurePort
                ),
                Handler = this,
                DependsOn =
                [
                    new ContractId<Network>(""),
                    contract.Domain
                ]
            },
            [
                new Container(contract.Container.Name)
                {
                    Labels = new Dictionary<string, Argument<string>>
                    {
                        ["traefik.enable"] = "true",
                        [$"traefik.http.routers.{traefikRouterName}.rule"] =
                            new(plan => $"Host(`{plan.GetContract(contract.Domain).Value}`)"),
                        [$"traefik.http.services.{traefikRouterName}.loadbalancer.server.port"] =
                            contract.Port.ToString(),
                        [$"traefik.http.routers.{traefikRouterName}.tls"] =
                            new(plan =>
                                GetCertificateResolver(plan.GetContract(contract.Domain)) == null
                                    ? "false"
                                    : "true"
                            ),
                        [$"traefik.http.routers.{traefikRouterName}.tls.certresolver"] =
                            new(plan =>
                                GetCertificateResolver(plan.GetContract(contract.Domain))
                            )
                    },
                    DependsOn = [contract, contract.Domain]
                }
            ]
        );
    }

    public override HttpEndpoint Install(HttpEndpoint contract, ExecutionPlan plan)
    {
        var container = plan.GetContract(contract.Container);
        var network = plan.GetContract(container.Network);
        Debug.Assert(network.Installed);

        _container.AttachNetwork(network.NetworkName);
        
        return contract with
        {
            NetworkName = network.NetworkName,
        };
    }

    public override void Uninstall(HttpEndpoint contract)
    {
        Debug.Assert(contract.Installed);
        Debug.Assert(contract.NetworkName != null);

        _container.DetachNetwork(contract.NetworkName);
    }

    /// <summary>
    /// Gets traefik certificate resolver, which can be used for the specific domain
    /// </summary>
    private string? GetCertificateResolver(Domain domain)
    {
        if (domain.IsInternal != false)
        {
            return null;
        }

        if (_webPort == 80)
        {
            // ReSharper disable once StringLiteralTypo
            return "httpchallenge";
        }

        if (_webSecurePort == 443)
        {
            // ReSharper disable once StringLiteralTypo
            return "tlschallenge";
        }

        return null;
    }
}