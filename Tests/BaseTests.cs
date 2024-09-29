using Bogus;
using Frierun.Server;
using Frierun.Server.Models;
using Frierun.Tests.Factories;
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
            RegisterServices(services);
            Provider = services.BuildServiceProvider();
        }
        return Provider.GetRequiredService<T>();
    }
    
    protected virtual void RegisterServices(IServiceCollection services)
    {
        services.AddLogging();
        services.RegisterServices();

        services.AddSingleton<Faker<Package>, PackageFactory>();
        services.AddSingleton<Faker<Application>, ApplicationFactory>();
    }
    
    /// <summary>
    /// Gets factory for generating test data.
    /// </summary>
    protected Faker<T> GetFactory<T>()
        where T : class
    {
        return GetService<Faker<T>>();
    }
}