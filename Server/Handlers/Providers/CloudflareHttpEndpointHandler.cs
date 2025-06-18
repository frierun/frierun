using System.Diagnostics;
using System.Text.Json.Nodes;
using Frierun.Server.Data;

namespace Frierun.Server.Handlers;

public class CloudflareHttpEndpointHandler(Application application, ICloudflareClient client)
    : Handler<HttpEndpoint>(application)
{
    private readonly Container _container = application.Contracts.OfType<Container>().Single();
    private readonly CloudflareTunnel _tunnel = application.Contracts.OfType<CloudflareTunnel>().Single();

    public override IEnumerable<ContractInitializeResult> Initialize(HttpEndpoint contract, string prefix)
    {
        var zones = client.GetZones();
        (string id, string name) zone;
        if (contract.ResultHost == null)
        {
            zone = zones.FirstOrDefault();
            if (zone == default)
            {
                throw new HandlerException(
                    "No DNS zones found in Cloudflare.",
                    "Please ensure you have at least one DNS zone configured in your Cloudflare account.",
                    contract
                );
            }

            contract = contract with
            {
                ResultHost = FindUniqueName(
                    prefix,
                    c => c.ResultHost,
                    $".{zone.name}"
                )
            };
        }
        else
        {
            var rootDomain = contract.ResultHost.Split('.', 2).Last();
            zone = zones.FirstOrDefault(tuple => tuple.name == rootDomain || tuple.name == contract.ResultHost);
            if (zone == default)
            {
                throw new HandlerException(
                    $"No DNS zone found for {contract.ResultHost} in Cloudflare.",
                    "Please ensure the domain is configured in your Cloudflare account.",
                    contract
                );
            }
        }

        yield return new ContractInitializeResult(
            contract with
            {
                Handler = this,
                ResultSsl = true,
                ResultPort = 443,
                CloudflareZoneId = zone.id,
                DependsOn = contract.DependsOn.Append(new Network("")),
                DependencyOf = contract.DependencyOf.Append(contract.Container),
            }
        );
    }

    public override HttpEndpoint Install(HttpEndpoint contract, ExecutionPlan plan)
    {
        var container = plan.GetContract(contract.Container);
        var network = plan.GetContract(container.Network);
        Debug.Assert(network.Installed);
        Debug.Assert(_tunnel.Installed);
        Debug.Assert(contract.CloudflareZoneId != null);
        
        var config = client.GetTunnelConfiguration(_tunnel.AccountId, _tunnel.TunnelId);
        if (config["ingress"] is not JsonArray ingress || ingress.Count == 0)
        {
            ingress =
            [
                new JsonObject
                {
                    ["service"] = "http_status:404"
                }
            ];
            config["ingress"] = ingress;
        }

        ingress.Insert(
            0,
            new JsonObject
            {
                ["hostname"] = contract.ResultHost,
                ["service"] = $"http://{container.ContainerName}:{contract.Port}"
            }
        );
        
        client.UpdateTunnelConfiguration(
            _tunnel.AccountId,
            _tunnel.TunnelId,
            config
        );
        
        DeleteOldDnsRecords(contract.CloudflareZoneId, contract.ResultHost);
        client.CreateDnsRecord(contract.CloudflareZoneId, new JsonObject
        {
            ["type"] = "CNAME",
            ["name"] = contract.ResultHost,
            ["content"] = $"{_tunnel.TunnelId}.cfargotunnel.com",
            ["proxied"] = true
        });

        _container.AttachNetwork(network.NetworkName);
        return contract with
        {
            NetworkName = network.NetworkName
        };
    }
    
    private void DeleteOldDnsRecords(string cloudflareZoneId, string? resultHost)
    {
        if (resultHost == null)
        {
            return;
        }
        
        foreach (var record in client.GetDnsRecords(cloudflareZoneId))
        {
            if (record["name"]?.GetValue<string>() != resultHost)
            {
                continue;
            }

            var recordId = record["id"]?.GetValue<string>();
            if (recordId == null)
            {
                continue;
            }
            client.DeleteDnsRecord(cloudflareZoneId, recordId);
        }
    }

    public override void Uninstall(HttpEndpoint contract)
    {
        Debug.Assert(contract.Installed);
        Debug.Assert(contract.NetworkName != null);
        Debug.Assert(contract.CloudflareZoneId != null);
        Debug.Assert(_tunnel.Installed);        

        _container.DetachNetwork(contract.NetworkName);
        
        var config = client.GetTunnelConfiguration(_tunnel.AccountId, _tunnel.TunnelId);
        if (config["ingress"] is JsonArray ingress)
        {
            // Remove the ingress rule for this endpoint
            for (var i = 0; i < ingress.Count; i++)
            {
                if (ingress[i]?["hostname"]?.GetValue<string>() != contract.ResultHost)
                {
                    continue;
                }

                // Remove the rule that matches the service
                ingress.RemoveAt(i);
                i--;
            }
            
            client.UpdateTunnelConfiguration(_tunnel.AccountId, _tunnel.TunnelId, config);
        }

        DeleteOldDnsRecords(contract.CloudflareZoneId, contract.ResultHost);
    }
}