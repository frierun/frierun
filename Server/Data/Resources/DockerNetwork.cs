using System.Text.Json.Serialization;
using Frierun.Server.Installers;

namespace Frierun.Server.Data;

public class DockerNetwork : Resource
{
    public required string Name { get; init; }
}