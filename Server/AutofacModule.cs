using System.Reflection;
using Autofac;
using Docker.DotNet;
using Frierun.Server.Data;
using Frierun.Server.Handlers;
using Module = Autofac.Module;

namespace Frierun.Server;

public class AutofacModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        // Handlers
        builder.RegisterInstance<ProviderScopeBuilder>(
                static builder =>
                {
                    var assembly = Assembly.GetExecutingAssembly();
                    builder.RegisterAssemblyTypes(assembly)
                        .Where(type => type.Namespace?.StartsWith("Frierun.Server.Handlers.Base") == true)
                        .AsImplementedInterfaces()
                        .SingleInstance();
                }
            )
            .Named<ProviderScopeBuilder>("base")
            .SingleInstance();

        builder.RegisterInstance<ProviderScopeBuilder>(
                static builder =>
                {
                    builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
                        .Where(type => type.Namespace?.StartsWith("Frierun.Server.Handlers.Docker") == true)
                        .AsImplementedInterfaces()
                        .SingleInstance();

                    builder.RegisterType<DockerService>().AsSelf().SingleInstance();
                    builder.Register<IDockerClient>(
                            static context => context
                                .Resolve<Application>()
                                .Contracts
                                .OfType<DockerApiConnection>()
                                .Single()
                                .CreateClient()
                        )
                        .SingleInstance();
                }
            )
            .Named<ProviderScopeBuilder>("docker")
            .SingleInstance();
        
        builder.RegisterInstance<ProviderScopeBuilder>(
                static builder =>
                {
                    builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
                        .Where(type => type.Namespace?.StartsWith("Frierun.Server.Handlers.Udocker") == true)
                        .AsImplementedInterfaces()
                        .SingleInstance();
                }
            )
            .Named<ProviderScopeBuilder>("udocker")
            .SingleInstance();

        builder.RegisterInstance<ProviderScopeBuilder>(
                static builder =>
                {
                    builder.RegisterType<CloudflareHttpEndpointHandler>()
                        .AsImplementedInterfaces()
                        .SingleInstance();

                    builder.Register<ICloudflareClient>(
                            static context => context
                                .Resolve<Application>()
                                .Contracts
                                .OfType<CloudflareApiConnection>()
                                .Single()
                                .CreateClient()
                        )
                        .SingleInstance();
                }
            )
            .Named<ProviderScopeBuilder>("cloudflare-tunnel")
            .SingleInstance();

        builder.RegisterInstance<ProviderScopeBuilder>(
                static builder => builder.RegisterType<MysqlHandler>()
                    .AsImplementedInterfaces()
                    .SingleInstance()
            )
            .Named<ProviderScopeBuilder>("mysql")
            .SingleInstance();
        builder.RegisterInstance<ProviderScopeBuilder>(
                static builder => builder.RegisterType<MysqlHandler>()
                    .AsImplementedInterfaces()
                    .SingleInstance()
            )
            .Named<ProviderScopeBuilder>("mariadb")
            .SingleInstance();
        builder.RegisterInstance<ProviderScopeBuilder>(
                static builder => builder.RegisterType<PostgresqlHandler>()
                    .AsImplementedInterfaces()
                    .SingleInstance()
            )
            .Named<ProviderScopeBuilder>("postgresql")
            .SingleInstance();
        builder.RegisterInstance<ProviderScopeBuilder>(
                static builder => builder.RegisterType<StaticDomainHandler>()
                    .AsImplementedInterfaces()
                    .SingleInstance()
            )
            .Named<ProviderScopeBuilder>("static-zone")
            .SingleInstance();
        builder.RegisterInstance<ProviderScopeBuilder>(
                static builder => builder.RegisterType<TraefikHttpEndpointHandler>()
                    .AsImplementedInterfaces()
                    .SingleInstance()
            )
            .Named<ProviderScopeBuilder>("traefik")
            .SingleInstance();

        // Services
        builder.RegisterType<ContractRegistry>().AsSelf().SingleInstance();
        builder.RegisterType<ExecutionService>().AsSelf().SingleInstance();
        builder.RegisterType<InstallService>().AsSelf().SingleInstance();
        builder.RegisterType<PackageRegistry>().AsSelf().SingleInstance();
        builder.RegisterType<HandlerRegistry>().AsSelf().SingleInstance();
        builder.RegisterType<StateManager>().AsSelf().SingleInstance();
        builder.RegisterType<UninstallService>().AsSelf().SingleInstance();

        // Serialization
        builder.Register<string>(_ => Path.Combine(Storage.DirectoryName, "state.json"))
            .Named<string>("stateFilePath")
            .SingleInstance();

        builder.RegisterType<StateSerializer>()
            .SingleInstance()
            .WithParameter(Autofac.Core.ResolvedParameter.ForNamed<string>("stateFilePath"));
        builder.RegisterType<PackageSerializer>().SingleInstance();


        builder.Register<State>(context => context.Resolve<StateSerializer>().Load()).AsSelf().SingleInstance();
    }
}