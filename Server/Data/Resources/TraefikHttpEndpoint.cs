namespace Frierun.Server.Data;

public record TraefikHttpEndpoint(string Domain, int Port) : GenericHttpEndpoint(new Uri($"http://{Domain}:{Port}"));