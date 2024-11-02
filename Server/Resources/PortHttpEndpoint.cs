namespace Frierun.Server.Resources;

public record PortHttpEndpoint(string Ip, int Port) : HttpEndpoint($"http://{Ip}:{Port}");
