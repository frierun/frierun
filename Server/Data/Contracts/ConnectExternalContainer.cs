using System.Text.Json.Serialization;

namespace Frierun.Server.Data;

public record ConnectExternalContainer(
    string ContainerName,
    string? NetworkName = null
) : Contract($"{ContainerName} to {NetworkName ?? ""}")
{
    public string NetworkName { get; init; } = NetworkName ?? "";
    [JsonIgnore] public ContractId<Network> NetworkId => new(NetworkName);    
}