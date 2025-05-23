using Docker.DotNet;
using Frierun.Server.Data;

namespace Frierun.Server.Handlers;

public interface IDockerApiConnectionHandler : IHandler<DockerApiConnection>
{
    /// <summary>
    /// Create a Docker client from the contract.
    /// </summary>
    IDockerClient CreateClient(DockerApiConnection contract);

    /// <summary>
    /// Gets the socket path in the root system, to be mounted in a container.
    /// </summary>
    string GetSocketRootPath(DockerApiConnection contract);
}