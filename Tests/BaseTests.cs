using Frierun.Server;
using Microsoft.Extensions.DependencyInjection;

namespace Frierun.Tests;

public abstract class BaseTests
{
    private ServiceProvider? Provider { get; set; }
    
    protected T GetService<T>()
        where T : notnull
    {
        if (Provider == null)
        {
            var services = new ServiceCollection();
            services.RegisterServices();
            Provider = services.BuildServiceProvider();
        }
        return Provider.GetRequiredService<T>();
    }
}