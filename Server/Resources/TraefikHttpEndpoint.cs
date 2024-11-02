namespace Frierun.Server.Resources;

public record TraefikHttpEndpoint(string Domain, int Port) : HttpEndpoint($"http://{Domain}:{Port}");