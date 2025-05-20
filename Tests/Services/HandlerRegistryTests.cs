using Frierun.Server;
using Frierun.Server.Data;
using Frierun.Server.Handlers;
using Frierun.Server.Handlers.Base;

namespace Frierun.Tests;

public class HandlerRegistryTests : BaseTests
{
    [Theory]
    [InlineData(typeof(Dependency), typeof(DependencyHandler))]
    [InlineData(typeof(HttpEndpoint), typeof(PortHttpEndpointHandler))]
    [InlineData(typeof(Parameter), typeof(ParameterHandler))]
    [InlineData(typeof(Package), typeof(PackageHandler))]
    [InlineData(typeof(Password), typeof(PasswordHandler))]
    [InlineData(typeof(Substitute), typeof(SubstituteHandler))]
    public void GetHandlers_StaticHandler_ReturnsHandler(Type contractType, Type handlerType)
    {
        var registry = Resolve<HandlerRegistry>();

        var result = registry.GetHandlers(contractType).ToList();

        Assert.Single(result);
        Assert.IsType(handlerType, result[0]);
    }

    [Fact]
    public void GetHandlers_WrongContract_ReturnsEmpty()
    {
        var registry = Resolve<HandlerRegistry>();

        var result = registry.GetHandlers(typeof(Application));

        Assert.Empty(result);
    }

    [Fact]
    public void GetHandlers_HasTraefik_ReturnsBothHandlers()
    {
        InstallPackage("docker");
        InstallPackage("traefik");
        var registry = Resolve<HandlerRegistry>();

        var result = registry.GetHandlers(typeof(HttpEndpoint)).ToList();

        Assert.Equal(2, result.Count);
        Assert.IsType<TraefikHttpEndpointHandler>(result[0]);
        Assert.IsType<PortHttpEndpointHandler>(result[1]);
    }

    [Fact]
    public void GetHandlers_AddTraefik_ReturnsBothHandlers()
    {
        var registry = Resolve<HandlerRegistry>();
        InstallPackage("docker");
        InstallPackage("traefik");

        var result = registry.GetHandlers(typeof(HttpEndpoint)).ToList();

        Assert.Equal(2, result.Count);
        Assert.IsType<TraefikHttpEndpointHandler>(result[0]);
        Assert.IsType<PortHttpEndpointHandler>(result[1]);
    }

    [Fact]
    public void GetHandlers_RemoveTraefik_ReturnsDefaultHandler()
    {
        InstallPackage("docker");
        var application = InstallPackage("traefik");
        var registry = Resolve<HandlerRegistry>();
        Resolve<State>().RemoveApplication(application);

        var result = registry.GetHandlers(typeof(HttpEndpoint)).ToList();

        Assert.Single(result);
        Assert.IsType<PortHttpEndpointHandler>(result[0]);
    }

    [Fact]
    public void GetHandlers_AddTraefikSeveralTimes_ReturnsBothHandlers()
    {
        var registry = Resolve<HandlerRegistry>();
        var uninstallService = Resolve<UninstallService>();
        InstallPackage("docker");
        var application = InstallPackage("traefik");
        uninstallService.Handle(application);
        var application2 = InstallPackage("traefik");

        var result = registry.GetHandlers(typeof(HttpEndpoint)).ToList();

        Assert.NotEqual(application, application2);
        Assert.Equal(2, result.Count);
        Assert.IsType<TraefikHttpEndpointHandler>(result[0]);
        Assert.IsType<PortHttpEndpointHandler>(result[1]);
    }

    public static IEnumerable<object[]> PackagesWithHandlers()
    {
        yield return ["traefik", typeof(TraefikHttpEndpointHandler), typeof(HttpEndpoint)];
        yield return ["mysql", typeof(MysqlHandler), typeof(Mysql)];
        yield return ["mariadb", typeof(MysqlHandler), typeof(Mysql)];
        yield return ["postgresql", typeof(PostgresqlHandler), typeof(Postgresql)];
    }

    [Theory]
    [MemberData(nameof(PackagesWithHandlers))]
    public void GetHandlers_InstallPackage_AddsHandler(string packageName, Type handlerType, Type contractType)
    {
        InstallPackage("docker");
        var handlerRegistry = Resolve<HandlerRegistry>();

        var application = InstallPackage(packageName);

        Assert.Equal(application.Name, packageName);
        var handler = handlerRegistry.GetHandlers(contractType).Single(handler => handler.Application == application);
        Assert.IsType(handlerType, handler);
    }

    [Theory]
    [MemberData(nameof(PackagesWithHandlers))]
    public void GetHandlers_UninstallPackage_RemovesHandler(
        string packageName,
        Type _,
        Type contractType
    )
    {
        InstallPackage("docker");
        var handlerRegistry = Resolve<HandlerRegistry>();
        var application = InstallPackage(packageName);

        Resolve<UninstallService>().Handle(application);

        Assert.DoesNotContain(
            handlerRegistry.GetHandlers(contractType),
            handler => handler.Application == application
        );
    }

    [Theory]
    [InlineData(typeof(DependencyHandler))]
    [InlineData(typeof(PackageHandler))]
    [InlineData(typeof(ParameterHandler))]
    [InlineData(typeof(PasswordHandler))]
    [InlineData(typeof(PortHttpEndpointHandler))]
    [InlineData(typeof(RedisHandler))]
    [InlineData(typeof(SelectorHandler))]
    [InlineData(typeof(SubstituteHandler))]
    public void GetHandler_StaticHandler_ReturnsHandler(Type handlerType)
    {
        var registry = Resolve<HandlerRegistry>();

        var result = registry.GetHandler(handlerType.Name);

        Assert.NotNull(result);
        Assert.IsType(handlerType, result);
    }


    [Fact]
    public void GetHandler_WrongResource_ReturnsNull()
    {
        var registry = Resolve<HandlerRegistry>();

        var result = registry.GetHandler("WrongType");

        Assert.Null(result);
    }
    
    [Theory]
    [MemberData(nameof(PackagesWithHandlers))]
    public void GetHandler_InstallPackage_AddsHandler(string packageName, Type handlerType, Type _)
    {
        InstallPackage("docker");
        var handlerRegistry = Resolve<HandlerRegistry>();
        Assert.Null(handlerRegistry.GetHandler(handlerType.Name, packageName));

        InstallPackage(packageName);

        Assert.NotNull(handlerRegistry.GetHandler(handlerType.Name, packageName));
    }

    [Theory]
    [MemberData(nameof(PackagesWithHandlers))]
    public void GetHandler_UninstallPackage_RemovesHandler(string packageName, Type handlerType, Type _)
    {
        InstallPackage("docker");
        var handlerRegistry = Resolve<HandlerRegistry>();
        var application = InstallPackage(packageName);

        Resolve<UninstallService>().Handle(application);

        Assert.Null(handlerRegistry.GetHandler(handlerType.Name, packageName));
    }
    
    [Fact]
    public void GetHandlers_DefaultConfiguration_ReturnsFakeDockerApiConnectionHandler()
    {
        var registry = Resolve<HandlerRegistry>();
        
        var handler = registry.GetHandlers(typeof(DockerApiConnection)).First();

        Assert.IsType<DockerApiConnectionHandler>(handler);
    }
}