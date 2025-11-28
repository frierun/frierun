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
        _host = Program.CreateHost();

        // clear state
        var state = Resolve<State>();
        foreach (var contract in state.Contracts.Values.ToList())
        {
            state.RemoveContract(contract);
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
    protected Application InstallPackage(string name, ContractList? overrides = null)
    {
        Resolve<PackageRegistry>().Load();
        var package = Resolve<PackageRegistry>().Find(name)
                      ?? throw new Exception($"Package {name} not found");
        
        return InstallPackage(package, overrides);
    }

    protected Application InstallPackage(Package package, ContractList? overrides = null)
    {
        var plan = Resolve<ExecutionService>().Create(package.CreateApplication(null, overrides));
        return Resolve<InstallService>().Handle(plan) ??
               throw new Exception($"Package {package.Name} not installed");
    }

    protected void UninstallApplication(Application application)
    {
        Resolve<UninstallService>().Handle(application);
    }
}