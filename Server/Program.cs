using Autofac;
using Autofac.Extensions.DependencyInjection;
using Frierun.Server.Data;

namespace Frierun.Server;

public static class Program
{
    public static int Main(string[] args)
    {
        return CreateHost().Services
            .GetRequiredService<Console>()
            .Run(args);
    }

    public static IHost CreateHost()
    {
        var builder = Host.CreateApplicationBuilder();
        builder.ConfigureContainer(
            new AutofacServiceProviderFactory(),
            autofacBuilder =>
            {
                autofacBuilder.RegisterModule(new AutofacModule());
            }
        );
        var host = builder.Build();

        // load packages
        host.Services.GetRequiredService<PackageRegistry>().Load();
        
        // bind the discovery service
        var discoverService = host.Services.GetRequiredService<DiscoveryService>();
        var state = host.Services.GetRequiredService<State>();
        state.ApplicationAdded += application => discoverService.Discover(application);

        return host;
    }
}