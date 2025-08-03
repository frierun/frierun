using Frierun.Server;
using Frierun.Server.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

[assembly: CollectionBehavior(DisableTestParallelization = true)]
namespace Tests.Integration;

public abstract class BaseTests : IDisposable
{
    private readonly IHost _host;
    
    protected BaseTests()
    {
        _host = Program.CreateHost([]);

        // clear state
        var state = Resolve<State>();
        foreach (var application in state.Applications.ToList())
        {
            state.RemoveApplication(application);
        }
    }

    public void Dispose()
    {
        _host.Dispose();
        GC.SuppressFinalize(this);
    }
    
    /// <summary>
    /// Resolve an object from the IHost.
    /// </summary>
    protected T Resolve<T>()
        where T : notnull
    {
        return _host.Services.GetRequiredService<T>();
    }    

    /// <summary>
    /// Installs package by name and returns application
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

    protected Application InstallPackage(Package package)
    {
        var plan = Resolve<ExecutionService>().Create(package);
        return Resolve<InstallService>().Handle(plan) ??
               throw new Exception($"Package {package.Name} not installed");
    }

    protected void UninstallApplication(Application application)
    {
        Resolve<UninstallService>().Handle(application);
    }
}