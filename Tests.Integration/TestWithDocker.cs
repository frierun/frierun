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
    protected readonly IDockerClient DockerClient;

    protected TestWithDocker()
    {
        _docker = InstallPackage("docker");
        DockerClient = _docker.Contracts.OfType<DockerApiConnection>().Single().CreateClient();
        DockerService = new DockerService(
            Resolve<ILogger<DockerService>>(),
            DockerClient
        );
    }

    public new void Dispose()
    {
        UninstallApplication(_docker);
        Assert.Empty(Resolve<State>().Applications);
        DockerService.Prune().Wait();
        
        base.Dispose();
        GC.SuppressFinalize(this);
    }
}