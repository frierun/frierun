using Frierun.Server.Data;
using Frierun.Server.Handlers;

namespace Frierun.Tests.Handlers.Udocker;

public class PortEndpointHandlerTests : BaseTests
{
    [Fact]
    public void Install_PrivilegedPort_CreatesUnprivilegedPort()
    {
        InstallPackage("termux-udocker");
        var container = Factory<Container>().Generate("udocker");
        var portEndpoint = Factory<PortEndpoint>().Generate() with
        {
            Port = 80,
            Container = new ContractId<Container>(container.Name)
        };
        var package = Factory<Package>().Generate() with { Contracts = [portEndpoint, container] };

        var application = InstallPackage(package);

        var installedPort = application.Contracts.OfType<PortEndpoint>().Single();
        Assert.True(installedPort.Installed);
        Assert.True(installedPort.ExternalPort >= 1024);
    }

    [Fact]
    public void Install_PrivilegedPortPinned_FailedToCreate()
    {
        InstallPackage("termux-udocker");
        var container = Factory<Container>().Generate("udocker");
        var portEndpoint = Factory<PortEndpoint>().Generate() with
        {
            Port = 80,
            ExternalPort = 80,
            Container = new ContractId<Container>(container.Name)
        };
        var package = Factory<Package>().Generate() with { Contracts = [portEndpoint, container] };

        Assert.Throws<HandlerNotFoundException>(() => InstallPackage(package));
    }
}