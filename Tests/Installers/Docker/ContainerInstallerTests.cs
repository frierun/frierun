using Frierun.Server.Data;

namespace Frierun.Tests.Installers.Docker;

public class ContainerInstallerTests : BaseTests
{
    [Fact]
    public void Install_Container_CreatesNetwork()
    {
        var package = Factory<Package>().Generate() with
        {
            Contracts = [Factory<Container>().Generate()]
        };
        
        var application = InstallPackage(package);

        Assert.NotNull(application);
        Assert.Single(application.Resources.OfType<DockerNetwork>());
    }
}