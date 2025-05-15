using System.Text.Json.Serialization;
using Frierun.Server.Installers;

namespace Frierun.Server.Data;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "Type")]
[JsonDerivedType(typeof(GenericHttpEndpoint), nameof(GenericHttpEndpoint))]
[JsonDerivedType(typeof(TraefikHttpEndpoint), nameof(TraefikHttpEndpoint))]
public class GenericHttpEndpoint : Resource
{
    [JsonConstructor]
    protected GenericHttpEndpoint(Lazy<IHandler> lazyHandler) : base(lazyHandler)
    {
    }

    public GenericHttpEndpoint(IHandler handler) : base(handler)
    {
    }
    
    public required Uri Url { get; init; }
    public string Host => Url.Host;
    public int Port => Url.Port;
    public bool Ssl => Url.Scheme == "https";
}