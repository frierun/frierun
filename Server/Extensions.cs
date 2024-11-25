using Frierun.Server.Data;
using Frierun.Server.Services;

namespace Frierun.Server;

public static class Extensions
{
    public static void RegisterServices(this IServiceCollection services)
    {
        // Providers
        services.AddSingleton<ApplicationProvider>();
        services.AddSingleton<ContainerProvider>();
        services.AddSingleton<FileProvider>();
        services.AddSingleton<MountProvider>();
        services.AddSingleton<NetworkProvider>();
        services.AddSingleton<PortEndpointProvider>();
        services.AddSingleton<PortHttpEndpointProvider>();
        //services.AddSingleton<TraefikHttpEndpointProvider>();
        services.AddSingleton<VolumeProvider>();
        
        // Services
        services.AddSingleton<DockerService>();
        services.AddSingleton<ExecutionService>();
        services.AddSingleton<InstallService>();
        services.AddSingleton<PackageRegistry>();
        services.AddSingleton<ProviderRegistry>();
        services.AddSingleton<StateManager>();
        services.AddSingleton<UninstallService>();

        // Services/Serialization
        services.AddSingleton<StateSerializer>();
        services.AddSingleton<PackageSerializer>();
        
        services.AddSingleton<State>(s => s.GetRequiredService<StateSerializer>().Load());
    }
}