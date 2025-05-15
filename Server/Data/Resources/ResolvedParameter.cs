using System.Text.Json.Serialization;
using Frierun.Server.Installers;

namespace Frierun.Server.Data;

public class ResolvedParameter : Resource
{
    [JsonConstructor]
    protected ResolvedParameter(Lazy<IHandler> lazyHandler) : base(lazyHandler)
    {
    }

    public ResolvedParameter(IHandler handler) : base(handler)
    {
    }
    
    public required string Name { get; init; }
    public required string Value { get; init; }
}