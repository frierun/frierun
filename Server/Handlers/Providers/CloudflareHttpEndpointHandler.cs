using Frierun.Server.Data;

namespace Frierun.Server.Handlers;

public class CloudflareHttpEndpointHandler(Application application, ICloudflareClient client)
    : Handler<HttpEndpoint>(application)
{
    
    public override IEnumerable<ContractInitializeResult> Initialize(HttpEndpoint contract, string prefix)
    {
        var zones = client.GetZones();
        if (contract.Url == null)
        {
            var zone = zones.FirstOrDefault();
            if (zone == default)
            {
                throw new HandlerException(
                    "No DNS zones found in Cloudflare.",
                    "Please ensure you have at least one DNS zone configured in your Cloudflare account.",
                    contract
                );
            }

            contract = contract with { Url = new Uri($"https://{prefix}.{zone.name}") };
        }
        yield break;
    }
}