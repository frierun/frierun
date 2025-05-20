using Docker.DotNet;
using Frierun.Server.Data;
using Frierun.Server.Handlers;

namespace Frierun.Tests;

public class FakeDockerApiConnectionHandler(IDockerClient dockerClient) : IDockerApiConnectionHandler
{
    public IEnumerable<ContractInitializeResult> Initialize(DockerApiConnection contract, string prefix)
    {
        yield return new ContractInitializeResult(contract with {Handler = this});
    }

    public IDockerClient CreateClient(DockerApiConnection contract)
    {
        return dockerClient;
    }
}