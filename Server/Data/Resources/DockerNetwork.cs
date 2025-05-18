using System.Text.Json.Serialization;

namespace Frierun.Server.Data;

public class DockerNetwork : Resource
{
    public required string Name { get; init; }
}