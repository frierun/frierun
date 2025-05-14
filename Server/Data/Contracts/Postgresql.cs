using System.Text.Json.Serialization;

namespace Frierun.Server.Data;

public record Postgresql(
    string? Name = null,
    string? DatabaseName = null,
    string? NetworkName = null,
    bool Admin = false,
    PostgresqlDatabase? Result = null
) : Contract(Name ?? ""), IHasResult<PostgresqlDatabase>
{
    public string NetworkName { get; init; } = NetworkName ?? "";
    [JsonIgnore] public ContractId<Network> NetworkId => new(NetworkName);
}