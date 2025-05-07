using System.Text.Json.Serialization;
using Frierun.Server.Installers;

namespace Frierun.Server.Data;

public class GeneratedPassword : Resource
{
    [JsonConstructor]
    protected GeneratedPassword(Lazy<IHandler> lazyHandler) : base(lazyHandler)
    {
    }

    public GeneratedPassword(IHandler handler) : this(new Lazy<IHandler>(handler))
    {
    }
    
    public required string Value { get; init; }
}