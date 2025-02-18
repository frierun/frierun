using Frierun.Server.Data;

namespace Frierun.Tests.Data;

public class ContainerProviderTests : BaseTests
{
    [Fact]
    public void Install_Container_CreatesNetwork()
    {
        var package = GetFactory<Package>().Generate() with
        {
            Contracts = [GetFactory<Container>().Generate()]
        };
        
        var application = InstallPackage(package);

        Assert.NotNull(application);
        var dockerContainer = application.DependsOn.OfType<DockerContainer>().First();
        Assert.Contains(dockerContainer.DependsOn, r => r is DockerNetwork);
    }
}