using Frierun.Server.Data;

namespace Frierun.Server.Handlers.Base;

public class CloudflareApiConnectionHandler : Handler<CloudflareApiConnection>, ICloudflareApiConnectionHandler
{
    public CloudflareClient CreateClient(CloudflareApiConnection contract)
    {
        return new CloudflareClient();
    }
}