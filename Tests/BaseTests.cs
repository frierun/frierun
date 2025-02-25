using System.Reflection;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Bogus;
using Docker.DotNet;
using Docker.DotNet.Models;
using Frierun.Server;
using Frierun.Server.Data;
using Frierun.Server.Services;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Substitute = NSubstitute.Substitute;

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
        
        // mock docker client
        var dockerClient = Mock<IDockerClient>();
        dockerClient.Containers
            .CreateContainerAsync(default)
            .ReturnsForAnyArgs(Task.FromResult(new CreateContainerResponse()));
        dockerClient.Containers
            .StartContainerAsync(default, default)
            .ReturnsForAnyArgs(Task.FromResult(true));
        dockerClient.Exec
            .ExecCreateContainerAsync(default, default)
            .ReturnsForAnyArgs(Task.FromResult(new ContainerExecCreateResponse()
            {
                ID = "execId"
            }));

        dockerClient.Exec
            .StartAndAttachContainerExecAsync(default, default)
            .ReturnsForAnyArgs(Task.FromResult(new MultiplexedStream(new MemoryStream(), false)));
        
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

    /// <summary>
    /// Creates mock service and registers it in the container.
    /// </summary>
    protected T Mock<T>()
        where T : class
    {
        var mock = Substitute.For<T>();
        GetContainerBuilder().RegisterInstance(mock).As<T>().SingleInstance();
        return mock;
    }

    /// <summary>
    /// Installs package and returns application.
    /// </summary>
    protected Application? InstallPackage(Package package)
    {
        var executionService = Resolve<ExecutionService>();
        var installService = Resolve<InstallService>();
        var plan = executionService.Create(package);
        return installService.Handle(plan);
    }
}