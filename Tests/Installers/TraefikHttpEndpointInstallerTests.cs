using Frierun.Server.Data;
using Frierun.Server.Services;

namespace Frierun.Tests.Installers;

public class TraefikHttpEndpointInstallerTests : BaseTests
{
    private readonly InstallerRegistry _installerRegistry;
    private readonly Application _application;

    /// <inheritdoc />
    public TraefikHttpEndpointInstallerTests()
    {
        Resolve<PackageRegistry>().Load();
        var package = Resolve<PackageRegistry>().Find("traefik")
                      ?? throw new Exception("Traefik package not found");

        _installerRegistry = Resolve<InstallerRegistry>();
        Assert.Empty(_installerRegistry.GetInstallers(typeof(HttpEndpoint), "TraefikHttpEndpointInstaller"));

        _application = InstallPackage(package)
                       ?? throw new Exception("Traefik application not installed");
    }

    [Fact]
    public void Install_TraefikPackage_AddsTraefikHttpEndpointInstaller()
    {
        Assert.Single(_installerRegistry.GetInstallers(typeof(HttpEndpoint), "TraefikHttpEndpointInstaller"));
        Assert.NotNull(_installerRegistry.GetUninstaller(typeof(TraefikHttpEndpoint)));
    }

    [Fact]
    public void Uninstall_TraefikPackage_RemovesTraefikHttpEndpointInstaller()
    {
        Resolve<UninstallService>().Handle(_application);

        Assert.Empty(_installerRegistry.GetInstallers(typeof(HttpEndpoint), "TraefikHttpEndpointInstaller"));
        Assert.Null(_installerRegistry.GetUninstaller(typeof(TraefikHttpEndpoint)));
    }

    [Fact]
    public void Install_ContainerWithHttpEndpoint_ContainerDependsOnTraefik()
    {
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