using System.Reflection;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Bogus;
using Frierun.Server;
using Frierun.Tests.Factories;
using Microsoft.Extensions.DependencyInjection;

namespace Frierun.Tests;

public abstract class BaseTests
{
    private IContainer? Provider { get; set; }
    private ContainerBuilder? ContainerBuilder { get; set; }
    
    /// <summary>
    /// Resolves object from the provider.
    /// </summary>
    protected T Resolve<T>()
        where T : notnull
    {
        Provider ??= GetServices().Build();
        return Provider.Resolve<T>();
    }
    
    private ContainerBuilder GetServices()
    {
        if (ContainerBuilder != null)
        {
            return ContainerBuilder;
        }
        
        ContainerBuilder = new ContainerBuilder();
        
        var services = new ServiceCollection();
        services.AddLogging();
        ContainerBuilder.Populate(services);

        ContainerBuilder.RegisterModule(new AutofacModule());
        ContainerBuilder.RegisterType<Faker>().SingleInstance();

        // find all factories
        foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
        {
            var iterator = type;
            while (iterator != null)
            {
                if (iterator.IsGenericType && iterator.GetGenericTypeDefinition() == typeof(Faker<>))
                {
                    ContainerBuilder.RegisterType(type).As(iterator).SingleInstance();
                    break;
                }
                
                iterator = iterator.BaseType;
            }
        }
        
        return ContainerBuilder;
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