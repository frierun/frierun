using System.Text.Json.Serialization;
using Frierun.Server.Installers;

namespace Frierun.Server.Data;

public class TraefikHttpEndpoint : GenericHttpEndpoint
{
    public required string NetworkName { get; init; }
}