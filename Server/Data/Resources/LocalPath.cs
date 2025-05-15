using System.Text.Json.Serialization;
using Frierun.Server.Installers;

namespace Frierun.Server.Data;

public class LocalPath : Resource
{
    public required string Path { get; init; }
}