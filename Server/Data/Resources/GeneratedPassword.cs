using System.Text.Json.Serialization;
using Frierun.Server.Installers;

namespace Frierun.Server.Data;

public class GeneratedPassword : Resource
{
    public required string Value { get; init; }
}