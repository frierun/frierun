using Frierun.Server.Data;
using Frierun.Server.Services;

namespace Frierun.Tests.Installers.Traefik;

public class TraefikHttpEndpointInstallerTests : BaseTests
{
    [Fact]
    public void Install_ContainerWithHttpEndpoint_ContainerDependsOnTraefik()
    {
        Resolve<PackageRegistry>().Load();
        var traefikPackage = Resolve<PackageRegistry>().Find("traefik");
        Assert.NotNull(traefikPackage);
        var traefikApplication = InstallPackage(traefikPackage);
        Assert.NotNull(traefikApplication);
        var container = GetFactory<Container>().Generate();
        var package = GetFactory<Package>().Generate() with
        {
            Contracts =
            [
                container,
                GetFactory<HttpEndpoint>().Generate() with {ContainerName = container.Name}
            ]
        };

        var application = InstallPackage(package);

        Assert.NotNull(application);
        var dockerContainer = application.DependsOn.OfType<DockerContainer>().First();
        Assert.Contains(dockerContainer.DependsOn, r => r is TraefikHttpEndpoint);
    }
}