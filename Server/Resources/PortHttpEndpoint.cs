namespace Frierun.Server.Resources;

public record PortHttpEndpoint(Guid Id, string Ip, int Port) : HttpEndpoint(Id, $"http://{Ip}:{Port}");
