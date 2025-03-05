using Frierun.Server;
using Frierun.Server.Data;
using Frierun.Server.Installers;
using Frierun.Server.Installers.Base;
using Frierun.Server.Services;

namespace Frierun.Tests.Services;

public class InstallerRegistryTests : BaseTests
{
    [Theory]
    [InlineData(typeof(Dependency), typeof(DependencyInstaller))]
    [InlineData(typeof(HttpEndpoint), typeof(PortHttpEndpointInstaller))]
    [InlineData(typeof(Parameter), typeof(ParameterInstaller))]
    [InlineData(typeof(Package), typeof(PackageInstaller))]
    [InlineData(typeof(Password), typeof(PasswordInstaller))]
    [InlineData(typeof(Substitute), typeof(SubstituteInstaller))]
    public void GetInstaller_StaticInstaller_ReturnsInstaller(Type contractType, Type installerType)
    {
        var registry = Resolve<InstallerRegistry>();

        var result = registry.GetInstallers(contractType).ToList();

        Assert.Single(result);
        Assert.IsType(installerType, result[0]);
    }
    
    [Fact]
    public void GetInstaller_WrongContract_ReturnsNull()
    {
        var registry = Resolve<InstallerRegistry>();

        var result = registry.GetInstallers(typeof(Application));

        Assert.Empty(result);
    }
    
    [Fact]
    public void GetInstaller_HasTraefik_ReturnsTraefikInstaller()
    {
        var package = new Package("traefik");
        var application = new Application("traefik", package);
        var state = Resolve<State>();
        state.AddResource(application);
        var registry = Resolve<InstallerRegistry>();

        var result = registry.GetInstallers(typeof(HttpEndpoint)).ToList();

        Assert.Equal(2, result.Count);
        Assert.IsType<TraefikHttpEndpointInstaller>(result[0]);
        Assert.IsType<PortHttpEndpointInstaller>(result[1]);
    }
    
    [Fact]
    public void GetInstaller_AddTraefik_ReturnsTraefikInstaller()
    {
        var package = new Package("traefik");
        var registry = Resolve<InstallerRegistry>();
        var application = new Application("traefik", package);
        var state = Resolve<State>();
        state.AddResource(application);

        var result = registry.GetInstallers(typeof(HttpEndpoint)).ToList();

        Assert.Equal(2, result.Count);
        Assert.IsType<TraefikHttpEndpointInstaller>(result[0]);
        Assert.IsType<PortHttpEndpointInstaller>(result[1]);
    }    
    
    [Fact]
    public void GetInstaller_RemoveTraefik_ReturnsDefaultInstaller()
    {
        var package = new Package("traefik");
        var application = new Application("traefik", package);
        var state = Resolve<State>();
        state.AddResource(application);
        var registry = Resolve<InstallerRegistry>();
        state.RemoveResource(application);

        var result = registry.GetInstallers(typeof(HttpEndpoint)).ToList();

        Assert.Single(result);
        Assert.IsType<PortHttpEndpointInstaller>(result[0]);
    }
    
    [Theory]
    [InlineData(typeof(Application), typeof(PackageInstaller))]
    [InlineData(typeof(ResolvedParameter), typeof(ParameterInstaller))]
    [InlineData(typeof(GeneratedPassword), typeof(PasswordInstaller))]
    [InlineData(typeof(GenericHttpEndpoint), typeof(PortHttpEndpointInstaller))]
    public void GetUninstaller_StaticInstaller_ReturnsUninstaller(Type resourceType, Type installerType)
    {
        var registry = Resolve<InstallerRegistry>();

        var result = registry.GetUninstaller(resourceType);

        Assert.NotNull(result);
        Assert.IsType(installerType, result);
    }
    
    [Fact]
    public void GetUninstaller_WrongResource_ReturnsNull()
    {
        var registry = Resolve<InstallerRegistry>();

        var result = registry.GetUninstaller(typeof(Package));

        Assert.Null(result);
    }
    
    [Fact]
    public void GetUninstaller_HasTraefik_ReturnsTraefikInstaller()
    {
        var package = new Package("traefik");
        var application = new Application("traefik", package);
        var state = Resolve<State>();
        state.AddResource(application);
        var registry = Resolve<InstallerRegistry>();

        var result = registry.GetUninstaller(typeof(TraefikHttpEndpoint));

        Assert.NotNull(result);
        Assert.IsType<TraefikHttpEndpointInstaller>(result);
    }
    
    [Fact]
    public void GetUninstaller_AddTraefik_ReturnsTraefikInstaller()
    {
        var package = new Package("traefik");
        var registry = Resolve<InstallerRegistry>();
        var application = new Application("traefik", package);
        var state = Resolve<State>();
        state.AddResource(application);

        var result = registry.GetUninstaller(typeof(TraefikHttpEndpoint));

        Assert.NotNull(result);
        Assert.IsType<TraefikHttpEndpointInstaller>(result);
    }    
    
    [Fact]
    public void GetUninstaller_RemoveTraefik_ReturnsDefaultInstaller()
    {
        var package = new Package("traefik");
        var application = new Application("traefik", package);
        var state = Resolve<State>();
        state.AddResource(application);
        var registry = Resolve<InstallerRegistry>();
        state.RemoveResource(application);

        var result = registry.GetUninstaller(typeof(TraefikHttpEndpoint));

        Assert.Null(result);
    }
}