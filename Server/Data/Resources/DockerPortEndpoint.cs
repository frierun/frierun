using System.Text.Json.Serialization;
using Frierun.Server.Installers;

namespace Frierun.Server.Data;

public class DockerPortEndpoint : Resource
{
    [JsonConstructor]
    protected DockerPortEndpoint(Lazy<IHandler> lazyHandler) : base(lazyHandler)
    {
    }

    public DockerPortEndpoint(IHandler handler) : base(handler)
    {
    }
    
    public required string Name { get; init; }
    public required string Ip { get; init; }
    public required int Port { get; init; }
    public required Protocol Protocol { get; init; }

    public string Url => $"{Protocol.ToString().ToLower()}://{Ip}:{Port}";
}