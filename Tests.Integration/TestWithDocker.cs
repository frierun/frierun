using Docker.DotNet;
using Frierun.Server;
using Frierun.Server.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Tests.Integration;

public abstract class TestWithDocker : BaseTests, IDisposable
{
    private readonly Application _docker;
    protected readonly DockerService DockerService;
    protected readonly DockerClient DockerClient;

    protected TestWithDocker()
    {
        _docker = InstallPackage("docker");
        DockerClient = new DockerClientConfiguration().CreateClient();
        DockerService = new DockerService(
            Services.GetRequiredService<ILogger<DockerService>>(),
            DockerClient
        );
    }

    public void Dispose()
    {
        UninstallApplication(_docker);
        Assert.Empty(Services.GetRequiredService<State>().Applications);
        DockerService.Prune().Wait();
    }
}