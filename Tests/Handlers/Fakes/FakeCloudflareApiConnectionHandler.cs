using Frierun.Server;
using Frierun.Server.Data;
using Frierun.Server.Handlers;
using NSubstitute;

namespace Frierun.Tests.Handlers;

public class FakeCloudflareApiConnectionHandler : Handler<CloudflareApiConnection>, ICloudflareApiConnectionHandler
{
    public ICloudflareClient Client { get; } = CreateClientSubstitute();

    public ICloudflareClient CreateClient(CloudflareApiConnection contract)
    {
        return Client;
    }

    /// <summary>
    /// Creates substitute for cloudflare client.
    /// </summary>
    private static ICloudflareClient CreateClientSubstitute()
    {
        var client = NSubstitute.Substitute.For<ICloudflareClient>();
        client.GetAccounts().ReturnsForAnyArgs(
            new List<(string id, string name)>
            {
                ("accountId1", "Account 1"),
                ("accountId2", "Account 2"),
            }
        );
        
        client.CreateTunnel(Arg.Any<string>(), Arg.Any<string>())
            .ReturnsForAnyArgs(_ => ("tunnelId", "tunnel token"));
        

        return client;
    }
}