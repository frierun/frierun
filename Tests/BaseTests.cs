using System.Reflection;
using Autofac;
using Autofac.Core;
using Autofac.Extensions.DependencyInjection;
using Bogus;
using Docker.DotNet;
using Frierun.Server;
using Frierun.Server.Data;
using Frierun.Server.Handlers;
using Frierun.Tests.Handlers;
using Microsoft.Extensions.DependencyInjection;
using Substitute = NSubstitute.Substitute;

namespace Frierun.Tests;

public abstract class BaseTests
{
    private IContainer? Provider { get; set; }
    private ContainerBuilder? ContainerBuilder { get; set; }
    protected IDockerClient DockerClient => Handler<FakeDockerApiConnectionHandler>().Client;
    protected ICloudflareClient CloudflareClient => Handler<FakeCloudflareApiConnectionHandler>().Client;

    /// <summary>
    /// Resolve object from the provider.
    /// </summary>
    protected T Resolve<T>()
        where T : notnull
    {
        Provider ??= GetContainerBuilder().Build();
        return Provider.Resolve<T>();
    }

    private ContainerBuilder GetContainerBuilder()
    {
        if (Provider != null)
        {
            throw new Exception("Can't configure container after it was built.");
        }

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
        ContainerBuilder.RegisterType<TemporaryFile>().SingleInstance();

        ContainerBuilder.Register<string>(context => context.Resolve<TemporaryFile>())
            .Named<string>("stateFilePath")
            .SingleInstance();

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

        // register fake handlers
        ContainerBuilder.RegisterDecorator<ProviderScopeBuilder>(
            (_, _, builder) =>
            {
                return b =>
                {
                    builder(b);
                    b.RegisterType<FakeDockerApiConnectionHandler>()
                        .AsImplementedInterfaces()
                        .SingleInstance()
                        .OnlyIf(
                            registryBuilder => registryBuilder.IsRegistered(new TypedService(typeof(IDockerApiConnectionHandler)))
                        );
                    b.RegisterType<FakeCloudflareApiConnectionHandler>()
                        .AsImplementedInterfaces()
                        .SingleInstance()
                        .OnlyIf(
                            registryBuilder => registryBuilder.IsRegistered(new TypedService(typeof(ICloudflareApiConnectionHandler)))
                        );
                    b.RegisterType<FakeSshConnectionHandler>()
                        .AsImplementedInterfaces()
                        .SingleInstance()
                        .OnlyIf(
                            registryBuilder => registryBuilder.IsRegistered(new TypedService(typeof(ISshConnectionHandler)))
                        );
                };
            }
        );

        return ContainerBuilder;
    }
    
    /// <summary>
    /// Resolve handler of the specified type
    /// </summary> 
    protected T Handler<T>(Application? application = null)
        where T : IHandler
    {
        var handler = Resolve<HandlerRegistry>().GetHandler(typeof(T).Name, application?.Name);
        if (handler is not T castedHandler)
        {
            throw new Exception($"Handler {typeof(T).Name} not found");
        }

        return castedHandler;
    }    

    /// <summary>
    /// Get factory for generating test data.
    /// </summary>
    protected Faker<T> Factory<T>()
        where T : class
    {
        return Resolve<Faker<T>>().Clone();
    }

    /// <summary>
    /// Create mock service and registers it in the container.
    /// </summary>
    protected T Mock<T, TService>(object?[]? constructorArguments = null)
        where T : class, TService
        where TService : class
    {
        var mock = Substitute.For<T>(constructorArguments);
        GetContainerBuilder().RegisterInstance(mock).As<TService>();
        return mock;
    }

    /// <summary>
    /// Install package by name and returns application. Throw exception if installation fails.
    /// </summary>
    protected Application InstallPackage(string name, IEnumerable<Contract>? overrides = null)
    {
        Resolve<PackageRegistry>().Load();
        var package = Resolve<PackageRegistry>().Find(name)
                      ?? throw new Exception($"Package {name} not found");

        if (overrides != null)
        {
            var overridePackage = new Package(name) { Contracts = overrides };
            package = (Package)package.Merge(overridePackage);
        }

        return InstallPackage(package);
    }

    /// <summary>
    /// Install package by name and returns application. Throw exception if installation fails.
    /// </summary>
    protected Application InstallPackage(Package package)
    {
        var executionService = Resolve<ExecutionService>();
        var installService = Resolve<InstallService>();
        var plan = executionService.Create(package);
        var application = installService.Handle(plan);
        if (application != null)
        {
            return application;
        }

        var stateManager = Resolve<StateManager>();
        if (stateManager.Exception != null)
        {
            throw stateManager.Exception;
        }
        throw new Exception($"Failed to install package {package.Name}");
    }

    /// <summary>
    /// Uninstall application.
    /// </summary>
    protected void UninstallApplication(Application application)
    {
        Resolve<UninstallService>().Handle(application);
    }
}