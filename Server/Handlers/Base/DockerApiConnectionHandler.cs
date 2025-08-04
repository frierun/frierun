using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json.Nodes;
using Docker.DotNet;
using Frierun.Server.Data;
using File = System.IO.File;

namespace Frierun.Server.Handlers.Base;

public class DockerApiConnectionHandler : Handler<DockerApiConnection>, IDockerApiConnectionHandler
{
    public override IEnumerable<DockerApiConnection> Discover()
    {
        var protocol = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "npipe" : "unix";
        var paths = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? new[] { "//./pipe/docker_engine", "//./pipe/podman-machine-default" }
            : new[] { "/var/run/docker.sock", "/run/podman/podman.sock" };

        foreach (var filePath in paths)
        {
            var socketPath = $"{protocol}:{filePath}";
            if (State.Contracts.OfType<DockerApiConnection>().Any(contract => contract.Path == socketPath))
            {
                continue;
            }

            if (!File.Exists(filePath))
            {
                continue;
            }

            DockerApiConnection? contract;
            try
            {
                contract = Verify(
                    new DockerApiConnection
                    {
                        Path = socketPath,
                        Handler = this
                    }
                );
            }
            catch (Exception)
            {
                continue;
            }

            yield return contract;
        }
    }

    public override DockerApiConnection Install(DockerApiConnection contract, ExecutionPlan plan)
    {
        try
        {
            return Verify(contract);
        }
        catch (Exception e)
        {
            throw new HandlerException(
                "Docker API connection failed.",
                "Specify correct path and make sure the docker daemon is running.",
                contract,
                e
            );
        }
    }

    /// <summary>
    /// Verifies the contract socket path and used engine
    /// </summary>
    private DockerApiConnection Verify(DockerApiConnection contract)
    {
        var client = CreateClient(contract);
        var version = client.System.GetVersionAsync().Result;
        var isPodman = version.Components[0].Name == "Podman Engine";

        return contract with
        {
            IsPodman = isPodman
        };
    }

    public IDockerClient CreateClient(DockerApiConnection contract)
    {
        var path = contract.Path ?? "";
        var configuration = path == ""
            ? new DockerClientConfiguration()
            : new DockerClientConfiguration(new Uri(path));

        return configuration.CreateClient();
    }

    public string GetSocketRootPath(DockerApiConnection contract)
    {
        if (contract.IsPodman != true)
        {
            return "/var/run/docker.sock";
        }

        var client = CreateClient(contract);
        var baseUri = (Uri)client
            .GetType()
            .GetField("_endpointBaseUri", BindingFlags.NonPublic | BindingFlags.Instance)!
            .GetValue(client)!;

        var httpClient = (HttpClient)client
            .GetType()
            .GetField("_client", BindingFlags.NonPublic | BindingFlags.Instance)!
            .GetValue(client)!;


        var result = httpClient.GetStringAsync(new Uri(baseUri, "v3.0.0/libpod/info")).Result;
        var json = JsonNode.Parse(result);
        var path = json?["host"]?["remoteSocket"]?["path"]?.GetValue<string>();
        var exists = json?["host"]?["remoteSocket"]?["exists"]?.GetValue<bool>();
        if (exists != true || path == null)
        {
            throw new HandlerException(
                "Podman API connection failed.",
                "Make sure the podman socket is enabled.",
                contract
            );
        }

        if (path.StartsWith("unix://"))
        {
            path = path["unix://".Length..];
        }

        return path;
    }
}