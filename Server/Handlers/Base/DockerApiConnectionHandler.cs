﻿using System.Reflection;
using System.Text.Json.Nodes;
using Docker.DotNet;
using Frierun.Server.Data;

namespace Frierun.Server.Handlers.Base;

public class DockerApiConnectionHandler : Handler<DockerApiConnection>, IDockerApiConnectionHandler
{
    public override DockerApiConnection Install(DockerApiConnection contract, ExecutionPlan plan)
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
                contract,
                e
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
            path = path.Substring("unix://".Length);
        }
        
        return path;
    }
}