using Frierun.Server;
using Frierun.Server.Data;
using Frierun.Server.Services;

namespace Frierun.Tests.Installers;

public class TraefikHttpEndpointInstallerTests : BaseTests
{
    [Fact]
    public void Install_ContainerWithHttpEndpoint_ContainerDependsOnTraefik()
    {
        var providerApplication = InstallPackage("traefik");
        Assert.NotNull(providerApplication);

        var container = Factory<Container>().Generate();
        var package = Factory<Package>().Generate() with
        {
            Contracts =
            [
                container,
                Factory<HttpEndpoint>().Generate() with { ContainerName = container.Name }
            ]
        };

        var application = InstallPackage(package);

        Assert.NotNull(application);
        var dockerContainer = application.DependsOn.OfType<DockerContainer>().First();
        Assert.Contains(dockerContainer.DependsOn, r => r is TraefikHttpEndpoint);
    }
}