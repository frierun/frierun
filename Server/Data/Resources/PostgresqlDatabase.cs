using System.Text.Json.Serialization;
using Frierun.Server.Installers;

namespace Frierun.Server.Data;

public class PostgresqlDatabase : Resource
{
    [JsonConstructor]
    protected PostgresqlDatabase(Lazy<IHandler> lazyHandler) : base(lazyHandler)
    {
    }

    public PostgresqlDatabase(IHandler handler) : this(new Lazy<IHandler>(handler))
    {
    }
    
    public required string User { get; init; }
    public required string Password { get; init; }
    public required string Host { get; init; }
    public string? Database { get; init; }
}