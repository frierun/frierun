using System.Text.Json.Serialization;

namespace Frierun.Server.Data;

public record Redis(
    string? Name = null,
    string? NetworkName = null
) : Contract(Name ?? "")
{
    public string NetworkName { get; init; } = NetworkName ?? "";
    [JsonIgnore] public ContractId<Network> NetworkId => new(NetworkName);
    
    public string ContainerName { get; init; } = "redis" + ((Name ?? "") == "" ? "" : $"-{Name}");
    [JsonIgnore] public ContractId<Container> ContainerId => new(ContainerName);
}