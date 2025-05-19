using System.Text.Json.Serialization;

namespace Frierun.Server.Data;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(GenericHttpEndpoint), nameof(GenericHttpEndpoint))]
[JsonDerivedType(typeof(TraefikHttpEndpoint), nameof(TraefikHttpEndpoint))]
public class GenericHttpEndpoint : Resource
{
    public required Uri Url { get; init; }
    public string Host => Url.Host;
    public int Port => Url.Port;
    public bool Ssl => Url.Scheme == "https";
}