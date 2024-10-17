using System.Reflection;
using Bogus;
using Frierun.Server;
using Frierun.Server.Resources;
using Frierun.Tests.Factories;
using Microsoft.Extensions.DependencyInjection;

namespace Frierun.Tests;

public abstract class BaseTests
{
    private ServiceProvider? Provider { get; set; }
    private IServiceCollection? Services { get; set; }
    
    /// <summary>
    /// Resolves object from the provider.
    /// </summary>
    protected T Resolve<T>()
        where T : notnull
    {
        Provider ??= GetServices().BuildServiceProvider();
        return Provider.GetRequiredService<T>();
    }
    
    private IServiceCollection GetServices()
    {
        if (Services != null)
        {
            return Services;
        }

        Services = new ServiceCollection();
        Services.AddLogging();
        Services.RegisterServices();

        // find all factories
        foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
        {
            var iterator = type;
            while (iterator != null)
            {
                if (iterator.IsGenericType && iterator.GetGenericTypeDefinition() == typeof(Faker<>))
                {
                    Services.AddSingleton(iterator, type);
                    break;
                }
                
                iterator = iterator.BaseType;
            }
        }
        
        return Services;
    }

    protected void RegisterServices(Action<IServiceCollection> action)
    {
        if (Provider != null)
        {
            throw new InvalidOperationException("Cannot register services after provider has been built.");
        }
        
        action(GetServices());
    }
    
    /// <summary>
    /// Gets factory for generating test data.
    /// </summary>
    protected Faker<T> GetFactory<T>()
        where T : class
    {
        return Resolve<Faker<T>>().Clone();
    }
}