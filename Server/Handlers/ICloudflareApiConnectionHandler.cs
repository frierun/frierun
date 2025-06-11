using Frierun.Server.Data;

namespace Frierun.Server.Handlers;

public interface ICloudflareApiConnectionHandler : IHandler
{
    /// <summary>
    /// Create a cloudflare client from the contract.
    /// </summary>
    ICloudflareClient CreateClient(CloudflareApiConnection contract);
}