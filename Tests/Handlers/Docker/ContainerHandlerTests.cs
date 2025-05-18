using Frierun.Server.Data;

namespace Frierun.Tests.Handlers.Docker;

public class ContainerHandlerTests : BaseTests
{
    [Fact]
    public void Install_Container_CreatesNetwork()
    {
        InstallPackage("docker");
        var package = Factory<Package>().Generate() with
        {
            Contracts = [Factory<Container>().Generate()]
        };
        
        var application = InstallPackage(package);

        Assert.True(application.Contracts.OfType<Network>().Single().Installed);
    }
}