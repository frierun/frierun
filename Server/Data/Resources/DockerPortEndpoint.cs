namespace Frierun.Server.Data;

public class DockerPortEndpoint : Resource
{
    public required string Name { get; init; }
    public required string Ip { get; init; }
    public required int Port { get; init; }
    public required Protocol Protocol { get; init; }

    public string Url => $"{Protocol.ToString().ToLower()}://{Ip}:{Port}";
}