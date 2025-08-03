using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting.Internal;

namespace Frierun.Server;

public class Program
{
    public static void Main(string[] args)
    {
        CreateHost(args).Services.GetRequiredService<Run>().Start();
    }

    public static IHost CreateHost(string[] args)
    {
        var builder = Host.CreateApplicationBuilder();
        builder.ConfigureContainer(
            new AutofacServiceProviderFactory(), 
            autofacBuilder =>
            {
                autofacBuilder.RegisterModule(new AutofacModule());
                autofacBuilder.RegisterInstance(args).AsSelf();
            }
        );
        var host = builder.Build();
        
        // load packages
        host.Services.GetRequiredService<PackageRegistry>().Load();

        return host;

    }
}