using System.Reflection;
using Autofac;
using Docker.DotNet;
using Frierun.Server.Data;
using Frierun.Server.Installers;
using Module = Autofac.Module;
using ResolvedParameter = Autofac.Core.ResolvedParameter;

namespace Frierun.Server;

public class AutofacModule : Module
{
    /// <inheritdoc />
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
                    builder.RegisterAssemblyTypes(assembly)
                        .Where(type => type.Namespace?.StartsWith("Frierun.Server.Installers.Docker") == true)
                        .AsImplementedInterfaces()
                        .SingleInstance();
                }
            )
            .Named<ProviderScopeBuilder>("base")
            .SingleInstance();

        // Package specific installers
        builder.RegisterInstance<ProviderScopeBuilder>(
                static builder => builder.RegisterType<MysqlInstaller>().As<IInstaller>().SingleInstance()
            )
            .Named<ProviderScopeBuilder>("mysql")
            .SingleInstance();
        builder.RegisterInstance<ProviderScopeBuilder>(
                static builder => builder.RegisterType<MysqlInstaller>().As<IInstaller>().SingleInstance()
            )
            .Named<ProviderScopeBuilder>("mariadb")
            .SingleInstance();
        builder.RegisterInstance<ProviderScopeBuilder>(
                static builder => builder.RegisterType<PostgresqlInstaller>().As<IInstaller>().SingleInstance()
            )
            .Named<ProviderScopeBuilder>("postgresql")
            .SingleInstance();
        builder.RegisterInstance<ProviderScopeBuilder>(
                static builder => builder.RegisterType<StaticDomainInstaller>().As<IInstaller>().SingleInstance()
            )
            .Named<ProviderScopeBuilder>("static-domain")
            .SingleInstance();
        builder.RegisterInstance<ProviderScopeBuilder>(
                static builder => builder.RegisterType<TraefikHttpEndpointInstaller>().As<IInstaller>().SingleInstance()
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

        // Docker
        builder.RegisterType<DockerService>().AsSelf().SingleInstance();
        builder.Register<IDockerClient>(
                //_ => new DockerClientConfiguration(new Uri("npipe://./pipe/podman-machine-default")).CreateClient()
                _ => new DockerClientConfiguration().CreateClient()
            )
            .SingleInstance();
        

        // Services/Serialization
        builder.Register<string>(_ => Path.Combine(Storage.DirectoryName, "state.json"))
            .Named<string>("stateFilePath")
            .SingleInstance();

        builder.RegisterType<StateSerializer>()
            .SingleInstance()
            .WithParameter(ResolvedParameter.ForNamed<string>("stateFilePath"));
        builder.RegisterType<PackageSerializer>().SingleInstance();


        builder.Register<State>(context => context.Resolve<StateSerializer>().Load()).AsSelf().SingleInstance();
    }
}