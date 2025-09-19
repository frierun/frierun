using Frierun.Server.Data;
using Frierun.Server.Handlers;
using NSubstitute;

namespace Frierun.Tests.Handlers.Base;

public class CloudflareTunnelHandlerTests : BaseTests
{
    [Fact]
    public void Install_CloudflareTunnel_Success()
    {
        InstallPackage("docker");

        var application = InstallPackage("cloudflare-tunnel");

        var cloudflareTunnel = Resolve<State>().GetContract<CloudflareTunnel>(application);
        Assert.Equal("accountId1", cloudflareTunnel.AccountId);
        Assert.NotNull(cloudflareTunnel.TunnelName);

        CloudflareClient.Received(1).CreateTunnel("accountId1", cloudflareTunnel.TunnelName);
        Assert.Equal("tunnel token", cloudflareTunnel.Token);

        var container = Resolve<State>().GetContract<Container>(application);
        Assert.Equal("cloudflare/cloudflared:latest", container.ImageName.Value);
        Assert.Equal(["tunnel", "--no-autoupdate", "run", "--token", "tunnel token"], container.Command.Value);
    }

    [Fact]
    public void Install_NoAccounts_ThrowsHandlerException()
    {
        InstallPackage("docker");
        CloudflareClient.GetAccounts().Returns([]);

        var exception = Assert.Throws<HandlerException>(() => InstallPackage("cloudflare-tunnel"));
        Assert.Equal("No Cloudflare accounts found.", exception.Message);
    }

    [Fact]
    public void Install_AccountIdIsSet_FoundAccount()
    {
        InstallPackage("docker");

        var application = InstallPackage(
            "cloudflare-tunnel",
            new ContractList
            {
                [""] = new CloudflareTunnel { AccountId = "accountId2" }
            }
        );

        var cloudflareTunnel = Resolve<State>().GetContract<CloudflareTunnel>(application);
        Assert.Equal("accountId2", cloudflareTunnel.AccountId);
    }

    [Fact]
    public void Install_IncorrectAccountId_ThrowsHandlerException()
    {
        InstallPackage("docker");

        var exception = Assert.Throws<HandlerException>(() => InstallPackage(
                "cloudflare-tunnel",
                new ContractList
                {
                    [""] = new CloudflareTunnel { AccountId = "invalidAccountId" }
                }
            )
        );
        Assert.Equal($"Account with ID invalidAccountId not found.", exception.Message);
    }

    [Fact]
    public void Uninstall_CloudflareTunnel_Success()
    {
        InstallPackage("docker");

        var application = InstallPackage("cloudflare-tunnel");

        Assert.NotNull(application);
        var cloudflareTunnel = Resolve<State>().GetContract<CloudflareTunnel>(application);
        var accountId = cloudflareTunnel.AccountId;
        var tunnelId = cloudflareTunnel.TunnelId;
        Assert.NotNull(accountId);
        Assert.NotNull(tunnelId);

        UninstallApplication(application);

        CloudflareClient.Received(1).DeleteTunnel(accountId, tunnelId);
    }
}