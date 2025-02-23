using Frierun.Server.Data;
using Frierun.Server.Services;

namespace Frierun.Tests.Installers.Traefik;

public class TraefikHttpEndpointInstallerTests : BaseTests
{
    [Fact]
    public void Install_TraefikPackage_AddsTraefikHttpEndpointInstaller()
    {
        Resolve<PackageRegistry>().Load();
        var traefikPackage = Resolve<PackageRegistry>().Find("traefik");
        Assert.NotNull(traefikPackage);
        var installerRegistry = Resolve<InstallerRegistry>();
        Assert.Null(installerRegistry.GetInstaller(typeof(HttpEndpoint), "TraefikHttpEndpointInstaller"));
        
        var traefikApplication = InstallPackage(traefikPackage);
        
        Assert.NotNull(traefikApplication);
        Assert.NotNull(installerRegistry.GetInstaller(typeof(HttpEndpoint), "TraefikHttpEndpointInstaller"));
    } 

    [Fact]
    public void Install_TraefikPackage_AddsTraefikHttpEndpointUninstaller()
    {
        Resolve<PackageRegistry>().Load();
        var traefikPackage = Resolve<PackageRegistry>().Find("traefik");
        Assert.NotNull(traefikPackage);
        var installerRegistry = Resolve<InstallerRegistry>();
        Assert.Null(installerRegistry.GetUninstaller(typeof(TraefikHttpEndpoint)));
        
        var traefikApplication = InstallPackage(traefikPackage);
        
        Assert.NotNull(traefikApplication);
        Assert.NotNull(installerRegistry.GetUninstaller(typeof(TraefikHttpEndpoint)));
    } 
    
    [Fact]
    public void Uninstall_TraefikPackage_RemovesTraefikHttpEndpointInstaller()
    {
        Resolve<PackageRegistry>().Load();
        var traefikPackage = Resolve<PackageRegistry>().Find("traefik");
        Assert.NotNull(traefikPackage);
        var installerRegistry = Resolve<InstallerRegistry>();
        var traefikApplication = InstallPackage(traefikPackage);
        Assert.NotNull(traefikApplication);
        Assert.NotNull(installerRegistry.GetInstaller(typeof(HttpEndpoint), "TraefikHttpEndpointInstaller"));
        var uninstallService = Resolve<UninstallService>();
        
        uninstallService.Handle(traefikApplication);
        
        Assert.Null(installerRegistry.GetInstaller(typeof(HttpEndpoint), "TraefikHttpEndpointInstaller"));
    } 

    [Fact]
    public void Uninstall_TraefikPackage_RemovesTraefikHttpEndpointUninstaller()
    {
        Resolve<PackageRegistry>().Load();
        var traefikPackage = Resolve<PackageRegistry>().Find("traefik");
        Assert.NotNull(traefikPackage);
        var installerRegistry = Resolve<InstallerRegistry>();
        var traefikApplication = InstallPackage(traefikPackage);
        Assert.NotNull(traefikApplication);
        Assert.NotNull(installerRegistry.GetUninstaller(typeof(TraefikHttpEndpoint)));
        var uninstallService = Resolve<UninstallService>();
        
        uninstallService.Handle(traefikApplication);
        
        Assert.Null(installerRegistry.GetUninstaller(typeof(TraefikHttpEndpoint)));
    }
    
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