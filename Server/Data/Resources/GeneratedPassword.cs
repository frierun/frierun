using System.Text.Json.Serialization;

namespace Frierun.Server.Data;

public class GeneratedPassword : Resource
{
    public required string Value { get; init; }
}