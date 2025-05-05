namespace Frierun.Server.Data;

public class GenericHttpEndpoint : Resource
{
    public virtual required Uri Url { get; init; }
    public string Host => Url.Host;
    public int Port => Url.Port;
    public bool Ssl => Url.Scheme == "https";
}