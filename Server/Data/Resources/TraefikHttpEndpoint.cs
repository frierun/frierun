using System.Text.Json.Serialization;

namespace Frierun.Server.Data;

public class TraefikHttpEndpoint : GenericHttpEndpoint
{
    public required string NetworkName { get; init; }
}