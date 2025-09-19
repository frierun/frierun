using Frierun.Server.Data;
using Frierun.Server.Handlers;

namespace Frierun.Tests.Handlers.Udocker;

public class NetworkHandlerTests : BaseTests
{
    public NetworkHandlerTests()
    {
        InstallPackage("termux-udocker");
    }

    [Fact]
    public void Install_DifferentPackages_HasSameNetworkName()
    {
        var package1 = Factory<Package>().Generate() with
        {
            Contracts = [Contract<Container>().Generate("udocker")]
        };
        var package2 = Factory<Package>().Generate() with
        {
            Contracts = [Contract<Container>().Generate("udocker")]
        };

        var application1 = InstallPackage(package1);
        var application2 = InstallPackage(package2);

        var network1 = State.GetContract<Network>(application1);
        var network2 = State.GetContract<Network>(application2);
        Assert.Equal(network1.NetworkName, network2.NetworkName);
    }

    [Fact]
    public void Install_WithNetworkName_FailsToInstall()
    {
        var package = Factory<Package>().Generate() with
        {
            Contracts = [Contract<Network>().Set(p => p.NetworkName, "test").Generate()]
        };

        Assert.Throws<HandlerNotFoundException>(() => InstallPackage(package));
    }
}