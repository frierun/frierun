using Docker.DotNet;
using Frierun.Server.Data;
using Frierun.Server.Handlers;

namespace Frierun.Tests;

public class FakeDockerApiConnectionHandler(IDockerClient dockerClient)
    : Handler<DockerApiConnection>, IDockerApiConnectionHandler
{
    public static string SocketRootPath = "/var/run/docker.sock";

    public IDockerClient CreateClient(DockerApiConnection contract)
    {
        return dockerClient;
    }

    public string GetSocketRootPath(DockerApiConnection contract)
    {
        return SocketRootPath;
    }
}