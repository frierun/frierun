using System.Text.Json.Serialization;
using Frierun.Server.Installers;

namespace Frierun.Server.Data;

public class ResolvedDomain : Resource
{
    [JsonConstructor]
    protected ResolvedDomain(Lazy<IHandler> lazyHandler) : base(lazyHandler)
    {
    }

    public ResolvedDomain(IHandler handler) : base(handler)
    {
    }
    
    public required string Value { get; init; } 
    public required bool IsInternal { get; init; }
}