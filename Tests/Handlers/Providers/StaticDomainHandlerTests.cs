using Frierun.Server.Data;

namespace Frierun.Tests.Handlers;

public class StaticDomainHandlerTests : BaseTests
{
    [Fact]
    public void Install_InternalDomainPackage_InstallInternalDomain()
    {
        InstallPackage("static-zone");
        var domain = Contract<Domain>().Generate();
        var package = Factory<Package>().Generate() with { Contracts = [domain] };

        var application = InstallPackage(package);

        var installedDomain = State.GetContract(application, domain.Ref);
        Assert.True(installedDomain.Installed);
        Assert.True(installedDomain.IsInternal);
    }

    [Fact]
    public void Install_ExternalDomainPackage_InstallExternalDomain()
    {
        InstallPackage(
            "static-zone",
            new ContractList { ["Internal"] = new Selector(Value: "No") }
        );
        var domain = Contract<Domain>().Generate();
        var package = Factory<Package>().Generate() with { Contracts = [domain] };

        var application = InstallPackage(package);

        var installedDomain = State.GetContract(application, domain.Ref);
        Assert.True(installedDomain.Installed);
        Assert.False(installedDomain.IsInternal);
    }
}