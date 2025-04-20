using System.Reflection;
using Docker.DotNet;
using Docker.DotNet.Models;
using Frierun.Server.Data;
using Microsoft.Extensions.DependencyInjection;

namespace Tests.Integration;

public class PackageTests : BaseTests
{
    public static IEnumerable<object[]> Packages()
    {
        var assemblyLocation = Assembly.GetExecutingAssembly().Location;
        var assemblyDirectory = Path.GetDirectoryName(assemblyLocation) ?? throw new InvalidOperationException();
        var packagesDirectory = Path.Combine(assemblyDirectory, "Packages");
        
        HashSet<string> ignoredPackages =
        [
            // internal
            "static-domain",
            
            // skip due to port 53 already in use
            "adguard",
            "pi-hole",
            "technitium",
            
            // require mysql
            "phpmyadmin",
            
            // require postgresql
            "authentik",
            "miniflux",
            "pgadmin",
        ];
        
        foreach (var fileName in Directory.EnumerateFiles(packagesDirectory, "*.yaml"))
        {
            var packageName = Path.GetFileNameWithoutExtension(fileName);
            if (ignoredPackages.Contains(packageName))
            {
                continue;
            }
            
            yield return [packageName];
        }
    }

    [Theory]
    [MemberData(nameof(Packages))]
    public async Task InstallingUninstalling_Package_ShouldCreateDockerContainer(string packageName)
    {
        var state = Services.GetRequiredService<State>();        
        var dockerClient = new DockerClientConfiguration().CreateClient();

        // install package
        var application = InstallPackage(packageName);

        Assert.NotNull(application);
        Assert.NotNull(state.Applications.FirstOrDefault(app => app.Name == packageName));
        var containers = await dockerClient.Containers.ListContainersAsync(new ContainersListParameters());
        Assert.NotEmpty(containers);
        Assert.All(
            containers, container =>
            {
                Assert.StartsWith($"/{packageName}", container.Names[0]);
                Assert.Equal("running", container.State);
            }
        );

        // uninstall package
        UninstallApplication(application);

        Assert.Empty(state.Applications);
        containers = await dockerClient.Containers.ListContainersAsync(new ContainersListParameters());
        Assert.Empty(containers);
    }
}