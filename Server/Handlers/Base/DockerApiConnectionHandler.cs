using Docker.DotNet;
using Frierun.Server.Data;

namespace Frierun.Server.Handlers.Base;

public class DockerApiConnectionHandler : IDockerApiConnectionHandler
{
    public IEnumerable<ContractInitializeResult> Initialize(DockerApiConnection contract, string prefix)
    {
        yield return new ContractInitializeResult(contract with { Handler = this });
    }

    public DockerApiConnection Install(DockerApiConnection contract, ExecutionPlan plan)
    {
        try
        {
            var client = CreateClient(contract);
            var version = client.System.GetVersionAsync().Result;
            var isPodman = version.Components[0].Name == "Podman Engine";

            return contract with
            {
                IsPodman = isPodman
            };
        }
        catch (Exception e)
        {
            throw new HandlerException(
                "Docker API connection failed.", 
                "Specify correct path and make sure the docker daemon is running.",
                contract
            );
        }
    }

    public IDockerClient CreateClient(DockerApiConnection contract)
    {
        var path = contract.Path ?? "";
        var configuration = path == ""
            ? new DockerClientConfiguration()
            : new DockerClientConfiguration(new Uri(path));

        return configuration.CreateClient();
    }
}