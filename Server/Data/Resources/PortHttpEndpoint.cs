namespace Frierun.Server.Data;

public record PortHttpEndpoint(string Ip, int Port) : HttpEndpoint($"http://{Ip}:{Port}");
