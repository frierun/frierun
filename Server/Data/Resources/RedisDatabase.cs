using System.Text.Json.Serialization;
using Frierun.Server.Installers;

namespace Frierun.Server.Data;

public class RedisDatabase : Resource
{
    public required string Host { get; init; }
}