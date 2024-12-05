namespace Frierun.Server.Data;

public record DockerPortEndpoint(string Ip, int Port, Protocol Protocol) : Resource;