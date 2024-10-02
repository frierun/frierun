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
    
    private void RegisterServices(IServiceCollection services)
    {
        services.AddLogging();
        services.RegisterServices();
        
        // clear state
        services.AddSingleton<State>(_ => new State());

        services.AddSingleton<Faker<Application>, ApplicationFactory>();
        services.AddSingleton<Faker<Package>, PackageFactory>();
        services.AddSingleton<Faker<Volume>, VolumeFactory>();
    }
    
    /// <summary>
    /// Gets factory for generating test data.
    /// </summary>
    protected Faker<T> GetFactory<T>()
        where T : class
    {
        return GetService<Faker<T>>().Clone();
    }
    
    /// <summary>
    /// Gets state
    /// </summary>
    protected State GetState()
    {
        var state = GetService<State>();
        state.Applications.Clear();
        return state;
    }

    
}