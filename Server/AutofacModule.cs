using System.Reflection;
using Autofac;
using Docker.DotNet;
using Frierun.Server.Data;
using Frierun.Server.Installers;
using Frierun.Server.Services;
using Module = Autofac.Module;
using ResolvedParameter = Autofac.Core.ResolvedParameter;

namespace Frierun.Server;

public class AutofacModule : Module 
{
    /// <inheritdoc />
    protected override void Load(ContainerBuilder builder)
    {
        var assembly = Assembly.GetExecutingAssembly();
        
        // Installers
        builder.RegisterAssemblyTypes(assembly)
            .Where(type => type.Namespace?.StartsWith("Frierun.Server.Installers.Base") == true)
            .AsImplementedInterfaces()
            .SingleInstance();
        builder.RegisterAssemblyTypes(assembly)
            .Where(type => type.Namespace?.StartsWith("Frierun.Server.Installers.Docker") == true)
            .AsImplementedInterfaces()
            .SingleInstance();

        // Package specific installers
        builder.RegisterType<TraefikHttpEndpointInstaller>()
            .Named<IInstaller>("traefik")
            .InstancePerDependency();
        builder.RegisterType<MysqlInstaller>()
            .Named<IInstaller>("mysql")
            .InstancePerDependency();
        builder.RegisterType<MysqlInstaller>()
            .Named<IInstaller>("mariadb")
            .InstancePerDependency();
        
        // Services
        builder.RegisterType<ContractRegistry>().AsSelf().SingleInstance();
        builder.RegisterType<DockerService>().AsSelf().SingleInstance();
        builder.RegisterType<ExecutionService>().AsSelf().SingleInstance();
        builder.RegisterType<InstallService>().AsSelf().SingleInstance();
        builder.RegisterType<PackageRegistry>().AsSelf().SingleInstance();
        builder.RegisterType<InstallerRegistry>().AsSelf().SingleInstance();
        builder.RegisterType<StateManager>().AsSelf().SingleInstance();
        builder.RegisterType<UninstallService>().AsSelf().SingleInstance();
        
        builder.Register<IDockerClient>(_ => new DockerClientConfiguration().CreateClient()).SingleInstance();

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