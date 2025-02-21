using Autofac;
using Docker.DotNet;
using Frierun.Server.Data;
using Frierun.Server.Installers.Base;
using Frierun.Server.Installers.Docker;
using Frierun.Server.Services;
using ResolvedParameter = Autofac.Core.ResolvedParameter;

namespace Frierun.Server;

public class AutofacModule : Module 
{
    /// <inheritdoc />
    protected override void Load(ContainerBuilder builder)
    {
        // Installers
        // Base
        builder.RegisterType<DependencyInstaller>().As<IInstaller>().SingleInstance();
        builder.RegisterType<PackageInstaller>().As<IInstaller>().SingleInstance();
        builder.RegisterType<ParameterInstaller>().As<IInstaller>().SingleInstance();
        builder.RegisterType<PasswordInstaller>().As<IInstaller>().SingleInstance();
        builder.RegisterType<PortHttpEndpointInstaller>().As<IInstaller>().SingleInstance();
        builder.RegisterType<SubstituteInstaller>().As<IInstaller>().SingleInstance();
        
        // Docker
        builder.RegisterType<ContainerInstaller>().As<IInstaller>().SingleInstance();
        builder.RegisterType<FileInstaller>().As<IInstaller>().SingleInstance();
        builder.RegisterType<MountInstaller>().As<IInstaller>().SingleInstance();
        builder.RegisterType<NetworkInstaller>().As<IInstaller>().SingleInstance();
        builder.RegisterType<PortEndpointInstaller>().As<IInstaller>().SingleInstance();
        builder.RegisterType<VolumeInstaller>().As<IInstaller>().SingleInstance();
        
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