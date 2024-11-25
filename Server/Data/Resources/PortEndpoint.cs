namespace Frierun.Server.Data;

public record PortEndpoint(string Ip, int Port, PortType PortType) : Resource;