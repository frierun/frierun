using Frierun.Server.Data;

namespace Frierun.Tests.Handlers;

public class StaticDomainHandlerTests : BaseTests
{
    [Fact]
    public void Install_InternalDomainPackage_InstallInternalDomain()
    {
        InstallPackage("static-domain");
        var package = Factory<Package>().Generate() with { Contracts = [new Domain()] };

        var application = InstallPackage(package);

        var domain = application.Contracts.OfType<Domain>().Single().Result;
        Assert.NotNull(domain);
        Assert.True(domain.IsInternal);
    }

    [Fact]
    public void Install_ExternalDomainPackage_InstallExternalDomain()
    {
        InstallPackage(
            "static-domain",
            [new Selector("Internal", SelectedOption: "No")]
        );
        var package = Factory<Package>().Generate() with { Contracts = [new Domain()] };

        var application = InstallPackage(package);

        var domain = application.Contracts.OfType<Domain>().Single().Result;
        Assert.NotNull(domain);
        Assert.False(domain.IsInternal);
    }
}