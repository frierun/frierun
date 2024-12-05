namespace Frierun.Server.Data;

public record TraefikHttpEndpoint(string Domain, int Port) : GenericHttpEndpoint($"http://{Domain}:{Port}");