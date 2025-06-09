using System.Text.Json.Nodes;

namespace Frierun.Server;

public interface ICloudflareClient
{
    /// <summary>
    /// Verify token
    /// </summary>
    /// <see href="https://developers.cloudflare.com/api/resources/user/subresources/tokens/" />
    bool VerifyToken();

    /// <summary>
    /// Get a list of accounts associated with the Cloudflare API token.
    /// </summary>
    /// <see href="https://developers.cloudflare.com/api/resources/accounts/methods/list/" />
    IEnumerable<(string id, string name)> GetAccounts();

    /// <summary>
    /// Create a new Cloudflare Tunnel.
    /// </summary>
    /// <see href="https://developers.cloudflare.com/api/resources/zero_trust/subresources/tunnels/subresources/cloudflared/methods/create/" />
    /// <returns>CloudflareTunnel</returns>
    (string id, string token) CreateTunnel(string accountId, string name);

    /// <summary>
    /// Get the configuration of an existing Cloudflare Tunnel.
    /// </summary>
    /// <see href="https://developers.cloudflare.com/api/resources/zero_trust/subresources/tunnels/subresources/cloudflared/subresources/configurations/methods/get/" />
    JsonObject GetTunnelConfiguration(string accountId, string tunnelId);

    /// <summary>
    /// Update the configuration of an existing Cloudflare Tunnel.
    /// </summary>
    /// <see href="https://developers.cloudflare.com/api/resources/zero_trust/subresources/tunnels/subresources/cloudflared/subresources/configurations/methods/update/"/>
    void UpdateTunnelConfiguration(string accountId, string tunnelId, JsonNode config);

    /// <summary>
    /// Delete a Cloudflare Tunnel.
    /// </summary>
    /// <see href="https://developers.cloudflare.com/api/resources/zero_trust/subresources/tunnels/subresources/cloudflared/methods/delete/" />
    void DeleteTunnel(string accountId, string tunnelId);

    /// <summary>
    /// Get a list of zones associated with the Cloudflare API token.
    /// </summary>
    /// <see href="https://developers.cloudflare.com/api/resources/zones/methods/list/" />
    IEnumerable<(string id, string name)> GetZones();

    /// <summary>
    /// Create a DNS record in a specified zone.
    /// </summary>
    /// <see href="https://developers.cloudflare.com/api/resources/dns/subresources/records/methods/create/" />
    void CreateDnsRecord(string zoneId, JsonObject record);

    /// <summary>
    /// List DNS records in a specified zone.
    /// </summary>
    /// <see href="https://developers.cloudflare.com/api/resources/dns/subresources/records/methods/list/" />
    IEnumerable<JsonObject> GetDnsRecords(string zoneId);
    
    /// <summary>
    /// Delete a DNS record in a specified zone.
    /// </summary>
    /// <see href="https://developers.cloudflare.com/api/resources/dns/subresources/records/methods/delete/" />
    void DeleteDnsRecord(string zoneId, string recordId);
}