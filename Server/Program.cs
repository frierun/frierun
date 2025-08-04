using Autofac;
using Autofac.Extensions.DependencyInjection;

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

        return host;
    }
}