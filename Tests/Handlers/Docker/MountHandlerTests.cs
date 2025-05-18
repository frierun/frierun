using Frierun.Server.Data;
using Mount = Frierun.Server.Data.Mount;

namespace Frierun.Tests.Handlers.Docker;

public class MountHandlerTests : BaseTests
{
    [Fact]
    public void Install_ContainerWithMount_CreatesVolume()
    {
        InstallPackage("docker");
        var container = Factory<Container>().Generate();
        var package = Factory<Package>().Generate() with
        {
            Contracts = [
                container,
                Factory<Mount>().Generate() with { ContainerName = container.Name }
            ]
        };
        
        var application = InstallPackage(package);

        Assert.True(application.Contracts.OfType<Volume>().Single().Installed);
    }
}