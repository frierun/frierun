using Frierun.Server.Data;

namespace Frierun.Tests.Installers.Docker;

public class ContainerInstallerTests : BaseTests
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

        Assert.Single(application.Resources.OfType<DockerNetwork>());
    }
}