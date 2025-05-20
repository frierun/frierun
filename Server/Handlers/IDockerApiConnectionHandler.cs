using Docker.DotNet;
using Frierun.Server.Data;

namespace Frierun.Server.Handlers;

public interface IDockerApiConnectionHandler : IHandler<DockerApiConnection>
{
    /// <summary>
    /// Create a Docker client from the contract.
    /// </summary>
    IDockerClient CreateClient(DockerApiConnection contract);
}