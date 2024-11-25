namespace Frierun.Server.Data;

public record PortEndpoint(string Ip, int Port, Protocol Protocol) : Resource;