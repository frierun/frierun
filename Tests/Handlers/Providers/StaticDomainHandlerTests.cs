using Frierun.Server.Data;

namespace Frierun.Tests.Handlers;

public class StaticDomainHandlerTests : BaseTests
{
    [Fact]
    public void Install_InternalDomainPackage_InstallInternalDomain()
    {
        InstallPackage("static-zone");
        var package = Factory<Package>().Generate() with { Contracts = [new Domain()] };

        var application = InstallPackage(package);

        var domain = application.Contracts.OfType<Domain>().Single();
        Assert.True(domain.Installed);
        Assert.True(domain.IsInternal);
    }

    [Fact]
    public void Install_ExternalDomainPackage_InstallExternalDomain()
    {
        InstallPackage(
            "static-zone",
            [new Selector("Internal", Value: "No")]
        );
        var package = Factory<Package>().Generate() with { Contracts = [new Domain()] };

        var application = InstallPackage(package);

        var domain = application.Contracts.OfType<Domain>().Single();
        Assert.True(domain.Installed);
        Assert.False(domain.IsInternal);
    }
}