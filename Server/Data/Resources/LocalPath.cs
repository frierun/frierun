using System.Text.Json.Serialization;

namespace Frierun.Server.Data;

public class LocalPath : Resource
{
    public required string Path { get; init; }
}