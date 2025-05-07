using System.Text.Json.Serialization;
using Frierun.Server.Installers;

namespace Frierun.Server.Data;

public class RedisDatabase : Resource
{
    [JsonConstructor]
    protected RedisDatabase(Lazy<IHandler> lazyHandler) : base(lazyHandler)
    {
    }

    public RedisDatabase(IHandler handler) : base(handler)
    {
    }
    
    public required string Host { get; init; }
}