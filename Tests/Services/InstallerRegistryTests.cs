using Frierun.Server;
using Frierun.Server.Data;
using Frierun.Server.Installers;
using Frierun.Server.Installers.Base;

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
    public void GetInstaller_HasTraefik_ReturnsBothInstallers()
    {
        var package = new Package("traefik");
        var application = new Application("traefik", package)
        {
            Resources = [
                new DockerContainer("traefik", "traefik"),
                new DockerPortEndpoint("127.0.0.1", 80, Protocol.Tcp)
            ]
        };
        var state = Resolve<State>();
        state.AddApplication(application);
        var registry = Resolve<InstallerRegistry>();

        var result = registry.GetInstallers(typeof(HttpEndpoint)).ToList();

        Assert.Equal(2, result.Count);
        Assert.IsType<TraefikHttpEndpointInstaller>(result[0]);
        Assert.IsType<PortHttpEndpointInstaller>(result[1]);
    }
    
    [Fact]
    public void GetInstaller_AddTraefik_ReturnsBothInstallers()
    {
        var package = new Package("traefik");
        var registry = Resolve<InstallerRegistry>();
        var application = new Application("traefik", package)
        {
            Resources = [
                new DockerContainer("traefik", "traefik"),
                new DockerPortEndpoint("127.0.0.1", 80, Protocol.Tcp)
            ]
        };
        var state = Resolve<State>();
        state.AddApplication(application);

        var result = registry.GetInstallers(typeof(HttpEndpoint)).ToList();

        Assert.Equal(2, result.Count);
        Assert.IsType<TraefikHttpEndpointInstaller>(result[0]);
        Assert.IsType<PortHttpEndpointInstaller>(result[1]);
    }    
    
    [Fact]
    public void GetInstaller_RemoveTraefik_ReturnsDefaultInstaller()
    {
        var package = new Package("traefik");
        var application = new Application("traefik", package)
        {
            Resources = [
                new DockerContainer("traefik", "traefik"),
                new DockerPortEndpoint("127.0.0.1", 80, Protocol.Tcp)
            ]
        };
        var state = Resolve<State>();
        state.AddApplication(application);
        var registry = Resolve<InstallerRegistry>();
        state.RemoveApplication(application);

        var result = registry.GetInstallers(typeof(HttpEndpoint)).ToList();

        Assert.Single(result);
        Assert.IsType<PortHttpEndpointInstaller>(result[0]);
    }

    public static IEnumerable<object[]> PackagesWithInstallers()
    {
        yield return ["traefik", typeof(TraefikHttpEndpointInstaller), typeof(HttpEndpoint)];
        yield return ["mysql", typeof(MysqlInstaller), typeof(Mysql)];
        yield return ["mariadb", typeof(MysqlInstaller), typeof(Mysql)];
        yield return ["postgresql", typeof(PostgresqlInstaller), typeof(Postgresql)];
    }

    [Theory]
    [MemberData(nameof(PackagesWithInstallers))]
    public void GetInstaller_InstallPackage_AddsInstaller(string packageName, Type installerType, Type contractType)
    {
        var installerRegistry = Resolve<InstallerRegistry>();
        Assert.Empty(installerRegistry.GetInstallers(contractType, installerType.Name));
        
        var application = InstallPackage(packageName);

        Assert.NotNull(application);
        Assert.Single(installerRegistry.GetInstallers(contractType, installerType.Name));
    }
    
    [Theory]
    [MemberData(nameof(PackagesWithInstallers))]
    public void GetInstaller_UninstallPackage_RemovesInstaller(string packageName, Type installerType, Type contractType)
    {
        var installerRegistry = Resolve<InstallerRegistry>();
        var application = InstallPackage(packageName);
        Assert.NotNull(application);

        Resolve<UninstallService>().Handle(application);
        
        Assert.Empty(installerRegistry.GetInstallers(contractType, installerType.Name));
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
    
    public static IEnumerable<object[]> PackagesWithUninstallers()
    {
        yield return ["traefik", typeof(TraefikHttpEndpoint)];
        yield return ["mysql", typeof(MysqlDatabase)];
        yield return ["mariadb", typeof(MysqlDatabase)];
        yield return ["postgresql", typeof(PostgresqlDatabase)];
    }

    [Theory]
    [MemberData(nameof(PackagesWithUninstallers))]
    public void GetUninstaller_InstallPackage_AddsInstaller(string packageName, Type resourceType)
    {
        var installerRegistry = Resolve<InstallerRegistry>();
        Assert.Null(installerRegistry.GetUninstaller(resourceType));
        
        var application = InstallPackage(packageName);

        Assert.NotNull(application);
        Assert.NotNull(installerRegistry.GetUninstaller(resourceType));
    }
    
    [Theory]
    [MemberData(nameof(PackagesWithUninstallers))]
    public void GetUninstaller_UninstallPackage_RemovesInstaller(string packageName,Type resourceType)
    {
        var installerRegistry = Resolve<InstallerRegistry>();
        var application = InstallPackage(packageName);
        Assert.NotNull(application);

        Resolve<UninstallService>().Handle(application);
        
        Assert.Null(installerRegistry.GetUninstaller(resourceType));
    }
}