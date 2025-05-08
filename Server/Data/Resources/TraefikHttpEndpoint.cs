using System.Text.Json.Serialization;
using Frierun.Server.Installers;

namespace Frierun.Server.Data;

public class TraefikHttpEndpoint : GenericHttpEndpoint
{
    [JsonConstructor]
    protected TraefikHttpEndpoint(Lazy<IHandler> lazyHandler) : base(lazyHandler)
    {
    }

    public TraefikHttpEndpoint(IHandler handler) : base(handler)
    {
    }
    
    public required string NetworkName { get; init; }
}