using System.Text.Json.Serialization;
using Frierun.Server.Installers;

namespace Frierun.Server.Data;

public class MysqlDatabase : Resource
{
    public required string User { get; init; }
    public required string Password { get; init; }
    public required string Host { get; init; }
    public string? Database { get; init; }
    public required string NetworkName { get; init; }
}