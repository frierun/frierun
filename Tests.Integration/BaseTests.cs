using Frierun.Server;
using Frierun.Server.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Tests.Integration;

public abstract class BaseTests
{
    private readonly WebApplicationFactory<Program> _factory = new();
    protected IServiceProvider Services => _factory.Services;

    protected BaseTests()
    {
        ClearState();
    }

    protected HttpClient CreateClient()
    {
        return _factory.CreateClient();
    }

    private void ClearState()
    {
        var state = Services.GetRequiredService<State>();
        foreach (var application in state.Applications.ToList())
        {
            state.RemoveApplication(application);
        }
    }

    /// <summary>
    /// Installs package by name and returns application
    /// </summary>
    protected Application InstallPackage(string name, IEnumerable<Contract>? overrides = null)
    {
        Services.GetRequiredService<PackageRegistry>().Load();
        var package = Services.GetRequiredService<PackageRegistry>().Find(name)
                      ?? throw new Exception($"Package {name} not found");

        if (overrides != null)
        {
            var overridePackage = new Package(name) {Contracts = overrides};
            package = (Package)package.With(overridePackage);
        }
        
        return InstallPackage(package);
    }

    protected Application InstallPackage(Package package)
    {
        var plan = Services.GetRequiredService<ExecutionService>().Create(package);
        return Services.GetRequiredService<InstallService>().Handle(plan) ??
               throw new Exception($"Package {package.Name} not installed");
    }

    protected void UninstallApplication(Application application)
    {
        Services.GetRequiredService<UninstallService>().Handle(application);
    }
}