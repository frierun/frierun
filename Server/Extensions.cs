using Frierun.Server.Models;
using Frierun.Server.Services;

namespace Frierun.Server;

public static class Extensions
{
    public static void RegisterServices(this IServiceCollection services)
    {
        services.AddSingleton<DockerService>();
        services.AddSingleton<InstallService>();
        services.AddSingleton<PackageRegistry>();
        services.AddSingleton<ParameterService>();
        services.AddSingleton<StateManager>();
        services.AddSingleton<UninstallService>();
        
        services.AddSingleton<State>(s => s.GetRequiredService<StateManager>().Load());
    }
}