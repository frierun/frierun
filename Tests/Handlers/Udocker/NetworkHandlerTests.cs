using Frierun.Server.Data;

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
        var package1 = Factory<Package>().Generate() with { Contracts = [Factory<Container>().Generate("udocker")] };
        var package2 = Factory<Package>().Generate() with { Contracts = [Factory<Container>().Generate("udocker")] };
        
        var application1 = InstallPackage(package1);
        var application2 = InstallPackage(package2);
        
        var network1 = application1.Contracts.OfType<Network>().Single();
        var network2 = application2.Contracts.OfType<Network>().Single();
        Assert.Equal(network1.NetworkName, network2.NetworkName);
    }
}