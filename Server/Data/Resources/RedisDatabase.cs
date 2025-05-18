using System.Text.Json.Serialization;

namespace Frierun.Server.Data;

public class RedisDatabase : Resource
{
    public required string Host { get; init; }
}