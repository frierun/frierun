using Frierun.Server.Data;

namespace Frierun.Tests.Installers;

public class StaticDomainInstallerTests : BaseTests
{
    [Fact]
    public void Install_InternalDomainPackage_InstallInternalDomain()
    {
        TryInstallPackage("static-domain");
        var package = Factory<Package>().Generate() with { Contracts = [new Domain()] };

        var application = TryInstallPackage(package);

        Assert.NotNull(application);
        var domain = application.Resources.OfType<ResolvedDomain>().First();
        Assert.True(domain.IsInternal);
    }

    [Fact]
    public void Install_ExternalDomainPackage_InstallExternalDomain()
    {
        TryInstallPackage(
            "static-domain",
            [new Selector("Internal", SelectedOption: "No")]
        );
        var package = Factory<Package>().Generate() with { Contracts = [new Domain()] };

        var application = TryInstallPackage(package);

        Assert.NotNull(application);
        var domain = application.Resources.OfType<ResolvedDomain>().First();
        Assert.False(domain.IsInternal);
    }
}