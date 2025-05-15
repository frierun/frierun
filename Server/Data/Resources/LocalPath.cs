using System.Text.Json.Serialization;
using Frierun.Server.Installers;

namespace Frierun.Server.Data;

public class LocalPath : Resource
{
    [JsonConstructor]
    protected LocalPath(Lazy<IHandler> lazyHandler) : base(lazyHandler)
    {
    }

    public LocalPath(IHandler handler) : base(handler)
    {
    }
    
    public required string Path { get; init; }
}