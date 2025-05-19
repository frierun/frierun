using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Frierun.Server.Data;

public record Redis(
    string? Name = null,
    string? NetworkName = null,
    string? Host = null
) : Contract(Name ?? "")
{
    [MemberNotNullWhen(true, nameof(Host))]
    public override bool Installed { get; init; }
    
    public string NetworkName { get; init; } = NetworkName ?? "";
    [JsonIgnore] public ContractId<Network> NetworkId => new(NetworkName);

    public string ContainerName { get; init; } = "redis" + ((Name ?? "") == "" ? "" : $"-{Name}");
    [JsonIgnore] public ContractId<Container> ContainerId => new(ContainerName);
}