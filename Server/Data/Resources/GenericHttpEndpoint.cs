namespace Frierun.Server.Data;

public record GenericHttpEndpoint(Uri Url) : Resource
{
    public string Host => Url.Host;
}