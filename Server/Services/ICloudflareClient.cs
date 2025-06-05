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
    /// Delete a Cloudflare Tunnel.
    /// </summary>
    /// <see href="https://developers.cloudflare.com/api/resources/zero_trust/subresources/tunnels/subresources/cloudflared/methods/delete/" />
    bool DeleteTunnel(string accountId, string tunnelId);
    
    /// <summary>
    /// Gets a list of zones associated with the Cloudflare API token.
    /// </summary>
    /// <see href="https://developers.cloudflare.com/api/resources/zones/methods/list/" />
    IEnumerable<(string id, string name)> GetZones();
}