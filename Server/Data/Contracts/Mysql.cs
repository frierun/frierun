using System.Text.Json.Serialization;

namespace Frierun.Server.Data;

public record Mysql(
    string? Name = null,
    string? DatabaseName = null,
    string? NetworkName = null,
    bool Admin = false
) : Contract(Name ?? "")
{
    public string NetworkName { get; init; } = NetworkName ?? "";
    [JsonIgnore] public ContractId<Network> NetworkId => new(NetworkName);
}