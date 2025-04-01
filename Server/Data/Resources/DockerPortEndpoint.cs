namespace Frierun.Server.Data;

public record DockerPortEndpoint(string Name, string Ip, int Port, Protocol Protocol) : Resource
{
    public string Url => $"{Protocol.ToString().ToLower()}://{Ip}:{Port}";
}
