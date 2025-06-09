using Docker.DotNet;
using Docker.DotNet.Models;
using Frierun.Server.Data;
using Frierun.Server.Handlers;
using NSubstitute;

namespace Frierun.Tests.Handlers;

public class FakeDockerApiConnectionHandler
    : Handler<DockerApiConnection>, IDockerApiConnectionHandler
{
    public string SocketRootPath { get; set; } = "/var/run/docker.sock";
    public IDockerClient DockerClient { get; } = CreateDockerSubstitute();

    public IDockerClient CreateClient(DockerApiConnection contract)
    {
        return DockerClient;
    }

    public string GetSocketRootPath(DockerApiConnection contract)
    {
        return SocketRootPath;
    }
    
    /// <summary>
    /// Creates substitute for docker client.
    /// </summary>
    private static IDockerClient CreateDockerSubstitute()
    {
        var dockerClient = NSubstitute.Substitute.For<IDockerClient>();
        dockerClient.Containers
            .CreateContainerAsync(default)
            .ReturnsForAnyArgs(Task.FromResult(new CreateContainerResponse {ID = "containerId"}));
        dockerClient.Containers
            .StartContainerAsync(default, default)
            .ReturnsForAnyArgs(Task.FromResult(true));
        dockerClient.Exec
            .ExecCreateContainerAsync(default, default)
            .ReturnsForAnyArgs(
                Task.FromResult(
                    new ContainerExecCreateResponse()
                    {
                        ID = "execId"
                    }
                )
            );

        dockerClient.Exec
            .StartAndAttachContainerExecAsync(default, default)
            .ReturnsForAnyArgs(Task.FromResult(new MultiplexedStream(new MemoryStream(), false)));

        return dockerClient;
    }    
}