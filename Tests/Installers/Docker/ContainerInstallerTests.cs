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
        var dockerContainer = application.DependsOn.OfType<DockerContainer>().First();
        Assert.Contains(dockerContainer.DependsOn, r => r is DockerNetwork);
    }
}