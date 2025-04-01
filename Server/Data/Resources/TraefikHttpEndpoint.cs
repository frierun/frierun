namespace Frierun.Server.Data;

public record TraefikHttpEndpoint(
    string Domain,
    int Port,
    bool Ssl = false
) : GenericHttpEndpoint(new Uri($"{(Ssl ? "https": "http")}://{Domain}:{Port}"));