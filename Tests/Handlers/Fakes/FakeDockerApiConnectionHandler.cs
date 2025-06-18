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
    public IDockerClient Client { get; } = CreateClientSubstitute();

    public IDockerClient CreateClient(DockerApiConnection contract)
    {
        return Client;
    }

    public string GetSocketRootPath(DockerApiConnection contract)
    {
        return SocketRootPath;
    }
    
    /// <summary>
    /// Creates substitute for docker client.
    /// </summary>
    private static IDockerClient CreateClientSubstitute()
    {
        var client = NSubstitute.Substitute.For<IDockerClient>();
        client.Containers
            .CreateContainerAsync(default)
            .ReturnsForAnyArgs(Task.FromResult(new CreateContainerResponse {ID = "containerId"}));
        client.Containers
            .StartContainerAsync(default, default)
            .ReturnsForAnyArgs(Task.FromResult(true));
        client.Exec
            .ExecCreateContainerAsync(default, default)
            .ReturnsForAnyArgs(
                Task.FromResult(
                    new ContainerExecCreateResponse()
                    {
                        ID = "execId"
                    }
                )
            );

        client.Exec
            .StartAndAttachContainerExecAsync(default, default)
            .ReturnsForAnyArgs(Task.FromResult(new MultiplexedStream(new MemoryStream(), false)));

        return client;
    }    
}