using System.Text.Json.Serialization;
using Frierun.Server.Installers;

namespace Frierun.Server.Data;

public class DockerContainer : Resource
{
    public required string Name { get; init; }
    public required string NetworkName { get; init; }
    

}