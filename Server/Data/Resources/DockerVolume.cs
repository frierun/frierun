using System.Text.Json.Serialization;

namespace Frierun.Server.Data;

public class DockerVolume : Resource
{
    public required string Name { get; init; }
}
