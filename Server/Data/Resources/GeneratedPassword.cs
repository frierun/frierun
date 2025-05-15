using System.Text.Json.Serialization;
using Frierun.Server.Installers;

namespace Frierun.Server.Data;

public class GeneratedPassword : Resource
{
    [JsonConstructor]
    protected GeneratedPassword(Lazy<IHandler> lazyHandler) : base(lazyHandler)
    {
    }

    public GeneratedPassword(IHandler handler) : base(handler)
    {
    }
    
    public required string Value { get; init; }
}