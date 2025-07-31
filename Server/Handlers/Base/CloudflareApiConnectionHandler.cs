using System.Diagnostics;
using Frierun.Server.Data;

namespace Frierun.Server.Handlers.Base;

public class CloudflareApiConnectionHandler : Handler<CloudflareApiConnection>, ICloudflareApiConnectionHandler
{
    public override CloudflareApiConnection Install(CloudflareApiConnection contract, ExecutionPlan plan)
    {
        try
        {
            var client = CreateClient(contract);
            if (!client.VerifyToken())
            {
                throw new Exception();
            }

            return contract;
        }
        catch (Exception)
        {
            throw new HandlerException(
                "Cloudflare API connection failed.",
                "Please check your API token and permissions.",
                contract
            );
        }
    }

    public ICloudflareClient CreateClient(CloudflareApiConnection contract)
    {
        Debug.Assert(contract.Installed);
        
        return new CloudflareClient(contract.Token);
    }
}