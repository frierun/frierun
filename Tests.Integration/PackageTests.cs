using System.Reflection;
using Docker.DotNet;
using Docker.DotNet.Models;
using Frierun.Server.Data;
using Frierun.Server.Services;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Tests.Integration;

public class PackageTests
{
    private readonly WebApplicationFactory<Program> _factory = new();

    public static IEnumerable<object[]> Packages()
    {
        var assemblyLocation = Assembly.GetExecutingAssembly().Location;
        var assemblyDirectory = Path.GetDirectoryName(assemblyLocation) ?? throw new InvalidOperationException();
        var packagesDirectory = Path.Combine(assemblyDirectory, "Packages");
        
        foreach (var fileName in Directory.EnumerateFiles(packagesDirectory, "*.json"))
        {
            var packageName = Path.GetFileNameWithoutExtension(fileName);
            if (packageName == "adguard" || packageName == "pi-hole")
            {
                // skip due to 53 port already in use
                continue;
            }
            yield return [packageName];
        }        
    }

    [Theory]
    [MemberData(nameof(Packages))]
    public async Task InstallingUninstalling_Package_ShouldCreateDockerContainer(string packageName)
    {
        var package = _factory.Services.GetRequiredService<PackageRegistry>().Find(packageName);
        Assert.NotNull(package);
        var executionService = _factory.Services.GetRequiredService<ExecutionService>();
        var installService = _factory.Services.GetRequiredService<InstallService>();
        var uninstallService = _factory.Services.GetRequiredService<UninstallService>();
        var state = _factory.Services.GetRequiredService<State>();
        foreach (var resource in state.Resources.ToList())
        {
            state.RemoveResource(resource);
        }
        var dockerClient = new DockerClientConfiguration().CreateClient();

        // install frierun package
        var plan = executionService.Create(package);
        var application = installService.Handle(plan);

        Assert.NotNull(application);
        Assert.NotNull(state.Resources.OfType<Application>().FirstOrDefault(app => app.Name == package.Name));
        var containers = await dockerClient.Containers.ListContainersAsync(new ContainersListParameters());
        Assert.NotEmpty(containers);
        Assert.All(
            containers, container =>
            {
                Assert.StartsWith($"/{package.Name}", container.Names[0]);
                Assert.Equal("running", container.State);
            }
        );

        // uninstall package
        uninstallService.Handle(application);

        Assert.Null(state.Resources.OfType<Application>().FirstOrDefault(app => app.Name == package.Name));
        containers = await dockerClient.Containers.ListContainersAsync(new ContainersListParameters());
        Assert.Empty(containers);
    }
}