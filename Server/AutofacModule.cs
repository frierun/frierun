using Autofac;
using Frierun.Server.Data;
using Frierun.Server.Services;

namespace Frierun.Server;

public class AutofacModule : Module 
{
    /// <inheritdoc />
    protected override void Load(ContainerBuilder builder)
    {
        // Providers
        builder.RegisterType<ApplicationProvider>().SingleInstance();
        builder.RegisterType<ContainerProvider>().SingleInstance();
        builder.RegisterType<FileProvider>().SingleInstance();
        builder.RegisterType<MountProvider>().SingleInstance();
        builder.RegisterType<NetworkProvider>().SingleInstance();
        builder.RegisterType<PortEndpointProvider>().SingleInstance();
        builder.RegisterType<PortHttpEndpointProvider>().SingleInstance();
        builder.RegisterType<VolumeProvider>().SingleInstance();
        
        // Services
        builder.RegisterType<DockerService>().AsSelf().SingleInstance();
        builder.RegisterType<ExecutionService>().AsSelf().SingleInstance();
        builder.RegisterType<InstallService>().AsSelf().SingleInstance();
        builder.RegisterType<PackageRegistry>().AsSelf().SingleInstance();
        builder.RegisterType<ProviderRegistry>().AsSelf().SingleInstance();
        builder.RegisterType<StateManager>().AsSelf().SingleInstance();
        builder.RegisterType<UninstallService>().AsSelf().SingleInstance();

        // Services/Serialization
        builder.RegisterType<StateSerializer>().SingleInstance();
        builder.RegisterType<PackageSerializer>().SingleInstance();
        
        builder.Register<State>(context => context.Resolve<StateSerializer>().Load()).AsSelf().SingleInstance();        
    }
}