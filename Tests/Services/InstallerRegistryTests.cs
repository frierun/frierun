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
        InstallPackage("traefik");
        var registry = Resolve<InstallerRegistry>();

        var result = registry.GetInstallers(typeof(HttpEndpoint)).ToList();

        Assert.Equal(2, result.Count);
        Assert.IsType<TraefikHttpEndpointInstaller>(result[0]);
        Assert.IsType<PortHttpEndpointInstaller>(result[1]);
    }

    [Fact]
    public void GetInstaller_AddTraefik_ReturnsBothInstallers()
    {
        var registry = Resolve<InstallerRegistry>();
        InstallPackage("traefik");

        var result = registry.GetInstallers(typeof(HttpEndpoint)).ToList();

        Assert.Equal(2, result.Count);
        Assert.IsType<TraefikHttpEndpointInstaller>(result[0]);
        Assert.IsType<PortHttpEndpointInstaller>(result[1]);
    }

    [Fact]
    public void GetInstaller_RemoveTraefik_ReturnsDefaultInstaller()
    {
        var application = InstallPackage("traefik") ?? throw new Exception("Application not found");
        var registry = Resolve<InstallerRegistry>();
        Resolve<State>().RemoveApplication(application);

        var result = registry.GetInstallers(typeof(HttpEndpoint)).ToList();

        Assert.Single(result);
        Assert.IsType<PortHttpEndpointInstaller>(result[0]);
    }
    
    [Fact]
    public void GetInstaller_AddTraefikSeveralTimes_ReturnsBothInstallers()
    {
        var registry = Resolve<InstallerRegistry>();
        var uninstallService = Resolve<UninstallService>();
        var application = InstallPackage("traefik");
        Assert.NotNull(application);
        uninstallService.Handle(application);
        var application2 = InstallPackage("traefik");

        var result = registry.GetInstallers(typeof(HttpEndpoint)).ToList();

        Assert.NotEqual(application, application2);
        Assert.Equal(2, result.Count);
        Assert.IsType<TraefikHttpEndpointInstaller>(result[0]);
        Assert.IsType<PortHttpEndpointInstaller>(result[1]);
    }
    

    [Fact]
    public void GetInstaller_TwoTraefik_FilteredByInstallerDefinition()
    {
        var application1 = InstallPackage("traefik") ?? throw new Exception("Application not found");
        var application2 = InstallPackage("traefik") ?? throw new Exception("Application not found");
        var registry = Resolve<InstallerRegistry>();

        Assert.Equal(3, registry.GetInstallers(typeof(HttpEndpoint)).Count());
        Assert.Equal(
            2,
            registry
                .GetInstallers(typeof(HttpEndpoint), new InstallerDefinition(nameof(TraefikHttpEndpointInstaller)))
                .Count()
        );
        Assert.Equal(
            2,
            registry
                .GetInstallers(typeof(HttpEndpoint), new InstallerDefinition(nameof(TraefikHttpEndpointInstaller)))
                .Count()
        );
        Assert.Single(
            registry
                .GetInstallers(
                    typeof(HttpEndpoint),
                    new InstallerDefinition(nameof(TraefikHttpEndpointInstaller), application1.Name)
                )
        );
        Assert.Single(
            registry
                .GetInstallers(
                    typeof(HttpEndpoint),
                    new InstallerDefinition(nameof(TraefikHttpEndpointInstaller), application2.Name)
                )
        );
        Assert.Empty(
            registry
                .GetInstallers(
                    typeof(HttpEndpoint),
                    new InstallerDefinition(nameof(TraefikHttpEndpointInstaller), "unknown application")
                )
        );
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
        Assert.Empty(
            installerRegistry.GetInstallers(
                contractType,
                new InstallerDefinition(installerType.Name, packageName)
            )
        );

        var application = InstallPackage(packageName);

        Assert.NotNull(application);
        Assert.Equal(application.Name, packageName);
        Assert.Single(
            installerRegistry.GetInstallers(
                contractType,
                new InstallerDefinition(installerType.Name, packageName)
            )
        );
    }

    [Theory]
    [MemberData(nameof(PackagesWithInstallers))]
    public void GetInstaller_UninstallPackage_RemovesInstaller(
        string packageName,
        Type installerType,
        Type contractType
    )
    {
        var installerRegistry = Resolve<InstallerRegistry>();
        var application = InstallPackage(packageName);
        Assert.NotNull(application);

        Resolve<UninstallService>().Handle(application);

        Assert.Empty(
            installerRegistry.GetInstallers(
                contractType,
                new InstallerDefinition(installerType.Name, application.Name)
            )
        );
    }
    
    [Theory]
    [InlineData(typeof(EmptyHandler))]
    public void GetHandler_StaticInstaller_ReturnsHandler(Type installerType)
    {
        var registry = Resolve<InstallerRegistry>();

        var result = registry.GetHandler(installerType.Name);

        Assert.NotNull(result);
        Assert.IsType(installerType, result);
    }
    
    
    [Fact]
    public void GetHandler_WrongResource_ReturnsNull()
    {
        var registry = Resolve<InstallerRegistry>();

        var result = registry.GetHandler("WrongType");

        Assert.Null(result);
    }

    public static IEnumerable<object[]> PackagesWithHandlers()
    {
        yield return ["mysql", typeof(MysqlInstaller)];
        yield return ["mariadb", typeof(MysqlInstaller)];
        yield return ["postgresql", typeof(PostgresqlInstaller)];
    }
    
    [Theory]
    [MemberData(nameof(PackagesWithHandlers))]
    public void GetHandler_InstallPackage_AddsHandler(string packageName, Type handlerType)
    {
        var installerRegistry = Resolve<InstallerRegistry>();
        Assert.Null(installerRegistry.GetHandler(handlerType.Name, packageName));

        var application = InstallPackage(packageName);

        Assert.NotNull(application);
        Assert.NotNull(installerRegistry.GetHandler(handlerType.Name, packageName));
    }
    
    [Theory]
    [MemberData(nameof(PackagesWithHandlers))]
    public void GetHandler_UninstallPackage_RemovesHandler(string packageName, Type handlerType)
    {
        var installerRegistry = Resolve<InstallerRegistry>();
        var application = InstallPackage(packageName);
        Assert.NotNull(application);

        Resolve<UninstallService>().Handle(application);

        Assert.Null(installerRegistry.GetHandler(handlerType.Name, packageName));
    }    
}