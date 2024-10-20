namespace Frierun.Server.Resources;

public record TraefikHttpEndpoint(Guid Id, string Domain, int Port) : HttpEndpoint(Id, $"http://{Domain}:{Port}");