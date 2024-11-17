namespace Frierun.Server.Data;

public record TraefikHttpEndpoint(string Domain, int Port) : HttpEndpoint($"http://{Domain}:{Port}");