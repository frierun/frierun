using Frierun.Server;
using Frierun.Server.Data;
using Frierun.Server.Handlers;

namespace Frierun.Tests.Handlers;

public class FakeCloudflareApiConnectionHandler : Handler<CloudflareApiConnection>, ICloudflareApiConnectionHandler
{
    public static ICloudflareClient Client { get; set; } = NSubstitute.Substitute.For<ICloudflareClient>();
    
    public ICloudflareClient CreateClient(CloudflareApiConnection contract)
    {
        return Client;
    }
}