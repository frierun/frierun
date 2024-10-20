using Frierun.Server.Models;
using Frierun.Server.Providers;
using Frierun.Server.Resources;
using Frierun.Server.Services;

namespace Frierun.Server;

public static class Extensions
{
    public static void RegisterServices(this IServiceCollection services)
    {
        // Providers
        services.AddSingleton<ApplicationProvider>();
        services.AddSingleton<ContainerProvider>();
        services.AddSingleton<HttpEndpointProvider>();
        services.AddSingleton<VolumeProvider>();
        
        // Services
        services.AddSingleton<DockerService>();
        services.AddSingleton<ExecutionService>();
        services.AddSingleton<InstallService>();
        services.AddSingleton<PackageRegistry>();
        services.AddSingleton<ParameterService>();
        services.AddSingleton<ProviderRegistry>();
        services.AddSingleton<StateManager>();
        services.AddSingleton<UninstallService>();

        // Services/Serialization
        services.AddSingleton<StateSerializer>();
        services.AddSingleton<PackageSerializer>();
        
        services.AddSingleton<State>(s => s.GetRequiredService<StateSerializer>().Load());
    }
}