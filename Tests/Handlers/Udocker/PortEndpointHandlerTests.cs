using Frierun.Server.Data;
using Frierun.Server.Handlers;

namespace Frierun.Tests.Handlers.Udocker;

public class PortEndpointHandlerTests : BaseTests
{
    [Fact]
    public void Install_PrivilegedPort_CreatesUnprivilegedPort()
    {
        InstallPackage("termux-udocker");
        var container = Contract<Container>().Generate("udocker");
        var portEndpoint = Contract<PortEndpoint>()
            .Set(p => p.Port, 80)
            .Set(p => p.Container, container.Id)
            .Generate();
        var package = Factory<Package>().Generate() with { Contracts = [portEndpoint, container] };

        var application = InstallPackage(package);

        var installedPort = application.GetContracts<PortEndpoint>().Single();
        Assert.True(installedPort.Installed);
        Assert.True(installedPort.ExternalPort >= 1024);
    }

    [Fact]
    public void Install_PrivilegedPortPinned_FailedToCreate()
    {
        InstallPackage("termux-udocker");
        var container = Contract<Container>().Generate("udocker");
        var portEndpoint = Contract<PortEndpoint>()
            .Set(p => p.Port, 80)
            .Set(p => p.ExternalPort, 80)
            .Set(p => p.Container, container.Id)
            .Generate();
        var package = Factory<Package>().Generate() with { Contracts = [portEndpoint, container] };

        Assert.Throws<HandlerNotFoundException>(() => InstallPackage(package));
    }
}