using Autofac;
using Frierun.Server.Data;
using Frierun.Server.Services;
using ResolvedParameter = Autofac.Core.ResolvedParameter;

namespace Frierun.Server;

public class AutofacModule : Module 
{
    /// <inheritdoc />
    protected override void Load(ContainerBuilder builder)
    {
        // Providers
        builder.RegisterType<ApplicationProvider>().As<IInstaller>().SingleInstance();
        builder.RegisterType<ContainerProvider>().As<IInstaller>().SingleInstance();
        builder.RegisterType<FileProvider>().As<IInstaller>().SingleInstance();
        builder.RegisterType<MountProvider>().As<IInstaller>().SingleInstance();
        builder.RegisterType<NetworkProvider>().As<IInstaller>().SingleInstance();
        builder.RegisterType<ParameterProvider>().As<IInstaller>().SingleInstance();
        builder.RegisterType<PortEndpointProvider>().As<IInstaller>().SingleInstance();
        builder.RegisterType<PortHttpEndpointProvider>().As<IInstaller>().SingleInstance();
        builder.RegisterType<SubstituteProvider>().As<IInstaller>().SingleInstance();
        builder.RegisterType<VolumeProvider>().As<IInstaller>().SingleInstance();
        
        // Services
        builder.RegisterType<ContractRegistry>().AsSelf().SingleInstance();
        builder.RegisterType<DockerService>().AsSelf().SingleInstance();
        builder.RegisterType<ExecutionService>().AsSelf().SingleInstance();
        builder.RegisterType<InstallService>().AsSelf().SingleInstance();
        builder.RegisterType<PackageRegistry>().AsSelf().SingleInstance();
        builder.RegisterType<ProviderRegistry>().AsSelf().SingleInstance();
        builder.RegisterType<StateManager>().AsSelf().SingleInstance();
        builder.RegisterType<UninstallService>().AsSelf().SingleInstance();

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