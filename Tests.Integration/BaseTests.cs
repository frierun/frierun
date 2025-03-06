using Frierun.Server;
using Frierun.Server.Data;
using Frierun.Server.Services;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Tests.Integration;

public class BaseTests
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
        foreach (var resource in state.Resources.ToList())
        {
            state.RemoveResource(resource);
        }
    }
    
    /// <summary>
    /// Installs package by name and returns application
    /// </summary>
    protected Application? InstallPackage(string name)
    {
        Services.GetRequiredService<PackageRegistry>().Load();
        var package = Services.GetRequiredService<PackageRegistry>().Find(name)
                      ?? throw new Exception($"Package {name} not found");

        return InstallPackage(package);
    }

    protected Application? InstallPackage(Package package)
    {
        var plan = Services.GetRequiredService<ExecutionService>().Create(package);
        return Services.GetRequiredService<InstallService>().Handle(plan);
    }
    
    protected void UninstallApplication(Application application)
    {
        Services.GetRequiredService<UninstallService>().Handle(application);
    }
}