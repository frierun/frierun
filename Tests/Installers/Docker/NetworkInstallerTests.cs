using Frierun.Server.Data;

namespace Frierun.Tests.Installers.Docker;

public class NetworkInstallerTests : BaseTests
{
    [Fact]
    public void Install_ContainerWithNetwork_ContainerDependsOnNetwork()
    {
        var network = GetFactory<Network>().Generate();
        var package = GetFactory<Package>().Generate() with
        {
            Contracts =
            [
                network,
                GetFactory<Container>().Generate() with { NetworkName = network.NetworkName! },
            ]
        };

        var application = InstallPackage(package);

        Assert.NotNull(application);
        var dockerContainer = application.DependsOn.OfType<DockerContainer>().First();
        Assert.Contains(dockerContainer.DependsOn, r => r is DockerNetwork);
    }
}