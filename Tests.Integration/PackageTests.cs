using System.Reflection;
using Docker.DotNet;
using Docker.DotNet.Models;
using Frierun.Server.Data;
using Frierun.Server.Services;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Tests.Integration;

public class PackageTests : BaseTests
{
    public static IEnumerable<object[]> Packages()
    {
        var assemblyLocation = Assembly.GetExecutingAssembly().Location;
        var assemblyDirectory = Path.GetDirectoryName(assemblyLocation) ?? throw new InvalidOperationException();
        var packagesDirectory = Path.Combine(assemblyDirectory, "Packages");
        
        foreach (var fileName in Directory.EnumerateFiles(packagesDirectory, "*.json"))
        {
            var packageName = Path.GetFileNameWithoutExtension(fileName);
            if (packageName is "adguard" or "pi-hole")
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
        var package = Services.GetRequiredService<PackageRegistry>().Find(packageName);
        Assert.NotNull(package);
        var state = Services.GetRequiredService<State>();        
        var dockerClient = new DockerClientConfiguration().CreateClient();

        // install package
        var application = InstallPackage(package);

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
        UninstallApplication(application);

        Assert.Null(state.Resources.OfType<Application>().FirstOrDefault(app => app.Name == package.Name));
        containers = await dockerClient.Containers.ListContainersAsync(new ContainersListParameters());
        Assert.Empty(containers);
    }
}