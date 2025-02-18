using Frierun.Server.Data;

namespace Frierun.Tests.Data;

public class PortEndpointProvider : BaseTests
{
    [Fact]
    public void Install_ContainerWithPortEndpoint_ContainerDependsOnPortEndpoint()
    {
        var container = GetFactory<Container>().Generate();
        var package = GetFactory<Package>().Generate() with
        {
            Contracts =
            [
                container,
                GetFactory<PortEndpoint>().Generate() with {ContainerName = container.Name}
            ]
        };

        var application = InstallPackage(package);

        Assert.NotNull(application);
        var dockerContainer = application.DependsOn.OfType<DockerContainer>().First();
        Assert.Contains(dockerContainer.DependsOn, r => r is DockerPortEndpoint);
    }
}