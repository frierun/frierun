using System.Reflection;
using Autofac;
using Autofac.Core;
using Autofac.Extensions.DependencyInjection;
using Autofac.Features.Metadata;
using Bogus;
using Docker.DotNet;
using Docker.DotNet.Models;
using Frierun.Server;
using Frierun.Server.Data;
using Frierun.Server.Installers;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Substitute = NSubstitute.Substitute;

namespace Frierun.Tests;

public abstract class BaseTests
{
    private IContainer? Provider { get; set; }
    private ContainerBuilder? ContainerBuilder { get; set; }
    protected IDockerClient DockerClient { get; } = CreateDockerSubstitute();

    /// <summary>
    /// Resolves object from the provider.
    /// </summary>
    protected T Resolve<T>()
        where T : notnull
    {
        Provider ??= GetContainerBuilder().Build();
        return Provider.Resolve<T>();
    }

    protected ContainerBuilder GetContainerBuilder()
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

        // mock docker client
        ContainerBuilder.RegisterInstance(DockerClient)
            .As<IDockerClient>()
            .SingleInstance();
        
        ContainerBuilder.RegisterDecorator<ProviderScopeBuilder>(
            (_, _, builder) =>
            {
                return b =>
                {
                    builder(b);
                    b.RegisterInstance(DockerClient)
                        .As<IDockerClient>()
                        .SingleInstance()
                        .OnlyIf(registryBuilder => registryBuilder.IsRegistered(new TypedService(typeof(IDockerClient))));
                };
            });
        
        return ContainerBuilder;
    }

    /// <summary>
    /// Creates substitute for docker client.
    /// </summary>
    private static IDockerClient CreateDockerSubstitute()
    {
        var dockerClient = Substitute.For<IDockerClient>();
        dockerClient.Containers
            .CreateContainerAsync(default)
            .ReturnsForAnyArgs(Task.FromResult(new CreateContainerResponse {ID = "containerId"}));
        dockerClient.Containers
            .StartContainerAsync(default, default)
            .ReturnsForAnyArgs(Task.FromResult(true));
        dockerClient.Exec
            .ExecCreateContainerAsync(default, default)
            .ReturnsForAnyArgs(
                Task.FromResult(
                    new ContainerExecCreateResponse()
                    {
                        ID = "execId"
                    }
                )
            );

        dockerClient.Exec
            .StartAndAttachContainerExecAsync(default, default)
            .ReturnsForAnyArgs(Task.FromResult(new MultiplexedStream(new MemoryStream(), false)));

        return dockerClient;
    }

    /// <summary>
    /// Gets factory for generating test data.
    /// </summary>
    protected Faker<T> Factory<T>()
        where T : class
    {
        return Resolve<Faker<T>>().Clone();
    }

    /// <summary>
    /// Creates mock service and registers it in the container.
    /// </summary>
    protected T Mock<T>()
        where T : class
    {
        return Mock<T, T>();
    }

    /// <summary>
    /// Creates mock service and registers it in the container.
    /// </summary>
    protected T Mock<T, TService>()
        where T : class, TService
        where TService : class
    {
        var mock = Substitute.For<T>();
        GetContainerBuilder().RegisterInstance(mock).As<TService>();
        return mock;
    }

    /// <summary>
    /// Installs package by name and returns application. Throws exception if installation fails.
    /// </summary>
    protected Application InstallPackage(string name, IEnumerable<Contract>? overrides = null)
    {
        Resolve<PackageRegistry>().Load();
        var package = Resolve<PackageRegistry>().Find(name)
                      ?? throw new Exception($"Package {name} not found");

        if (overrides != null)
        {
            var overridePackage = new Package(name) {Contracts = overrides};
            package = (Package)package.With(overridePackage);
        }
        
        return InstallPackage(package);
    }
    
    /// <summary>
    /// Installs package by name and returns application. Throws exception if installation fails.
    /// </summary>
    protected Application InstallPackage(Package package)
    {
        var executionService = Resolve<ExecutionService>();
        var installService = Resolve<InstallService>();
        var plan = executionService.Create(package);
        return installService.Handle(plan) ?? throw new Exception($"Failed to install package {package.Name}");
    }
}