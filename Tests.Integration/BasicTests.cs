using Docker.DotNet;
using Docker.DotNet.Models;
using Frierun.Server.Data;
using Frierun.Server.Services;
using Microsoft.Extensions.DependencyInjection;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace Tests.Integration;

using Microsoft.AspNetCore.Mvc.Testing;

public class BasicTests
{
    private readonly WebApplicationFactory<Program> _factory = new();

    [Theory]
    [InlineData("/api/v1/applications")]
    [InlineData("/api/v1/packages")]
    public async Task Get_Endpoints_ReturnSuccessAndCorrectContentType(string url)
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync(url);

        response.EnsureSuccessStatusCode();
        Assert.Equal("application/json; charset=utf-8", response.Content.Headers.ContentType?.ToString());
    }

    [Fact]
    public void Finding_Frierun_ShouldReturnPackage()
    {
        var package = _factory.Services.GetRequiredService<PackageRegistry>().Find("frierun");

        Assert.NotNull(package);
    }

    [Fact]
    public async Task InstallingUninstalling_Frierun_ShouldCreateDockerContainer()
    {
        var package = _factory.Services.GetRequiredService<PackageRegistry>().Find("frierun")!;
        var executionService = _factory.Services.GetRequiredService<ExecutionService>();
        var installService = _factory.Services.GetRequiredService<InstallService>();
        var uninstallService = _factory.Services.GetRequiredService<UninstallService>();
        var state = _factory.Services.GetRequiredService<State>();
        var dockerClient = new DockerClientConfiguration().CreateClient();

        // install frierun package
        var plan = executionService.Create(package);
        var application = installService.Handle(plan);

        Assert.NotNull(application);
        Assert.Contains(state.Resources, resource => resource is Application { Package.Name: "frierun" });
        var parameters = new ContainersListParameters
        {
            Filters = new Dictionary<string, IDictionary<string, bool>>
            {
                {
                    "name",
                    new Dictionary<string, bool>
                    {
                        { "frierun", true}
                    }
                }
            }
        };
        var containers = await dockerClient.Containers.ListContainersAsync(parameters);
        Assert.NotNull(containers);
        Assert.Single(containers);
        Assert.Equal("running", containers[0].State);
        
        // uninstall frierun package
        uninstallService.Handle(application);
        
        Assert.DoesNotContain(state.Resources, resource => resource is Application { Package.Name: "frierun" });
        containers = await dockerClient.Containers.ListContainersAsync(parameters);
        Assert.NotNull(containers);
        Assert.Empty(containers);
    }
}