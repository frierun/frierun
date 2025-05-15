using System.Reflection;
using Autofac;
using Docker.DotNet;
using Frierun.Server.Data;
using Frierun.Server.Installers;
using Module = Autofac.Module;

namespace Frierun.Server;

public class AutofacModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        // Installers
        builder.RegisterInstance<ProviderScopeBuilder>(
                static builder =>
                {
                    var assembly = Assembly.GetExecutingAssembly();
                    builder.RegisterAssemblyTypes(assembly)
                        .Where(type => type.Namespace?.StartsWith("Frierun.Server.Installers.Base") == true)
                        .AsImplementedInterfaces()
                        .SingleInstance();
                }
            )
            .Named<ProviderScopeBuilder>("base")
            .SingleInstance();

        // Package specific installers
        builder.RegisterInstance<ProviderScopeBuilder>(
                static builder =>
                {
                    var assembly = Assembly.GetExecutingAssembly();
                    builder.RegisterAssemblyTypes(assembly)
                        .Where(type => type.Namespace?.StartsWith("Frierun.Server.Installers.Docker") == true)
                        .AsImplementedInterfaces()
                        .SingleInstance();
                    
                    builder.RegisterType<DockerService>().AsSelf().SingleInstance();
                    builder.Register<IDockerClient>(
                            context =>
                            {
                                var application = context.Resolve<Application>();
                                var path = application.Contracts
                                    .OfType<Parameter>()
                                    .First(parameter => parameter.Name == "Path")
                                    .Result
                                    ?.Value ?? "";
                                
                                var configuration = path == "" 
                                    ? new DockerClientConfiguration() 
                                    : new DockerClientConfiguration(new Uri(path));
                                
                                return configuration.CreateClient();
                            }
                        )
                        .SingleInstance();
                }
            )
            .Named<ProviderScopeBuilder>("docker")
            .SingleInstance();
        
        builder.RegisterInstance<ProviderScopeBuilder>(
                static builder => builder.RegisterType<MysqlInstaller>()
                    .AsImplementedInterfaces()
                    .SingleInstance()
            )
            .Named<ProviderScopeBuilder>("mysql")
            .SingleInstance();
        builder.RegisterInstance<ProviderScopeBuilder>(
                static builder => builder.RegisterType<MysqlInstaller>()
                    .AsImplementedInterfaces()
                    .SingleInstance()
            )
            .Named<ProviderScopeBuilder>("mariadb")
            .SingleInstance();
        builder.RegisterInstance<ProviderScopeBuilder>(
                static builder => builder.RegisterType<PostgresqlInstaller>()
                    .AsImplementedInterfaces()
                    .SingleInstance()
            )
            .Named<ProviderScopeBuilder>("postgresql")
            .SingleInstance();
        builder.RegisterInstance<ProviderScopeBuilder>(
                static builder => builder.RegisterType<StaticDomainInstaller>()
                    .AsImplementedInterfaces()
                    .SingleInstance()
            )
            .Named<ProviderScopeBuilder>("static-domain")
            .SingleInstance();
        builder.RegisterInstance<ProviderScopeBuilder>(
                static builder => builder.RegisterType<TraefikHttpEndpointInstaller>()
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
        builder.RegisterType<InstallerRegistry>().AsSelf().SingleInstance();
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