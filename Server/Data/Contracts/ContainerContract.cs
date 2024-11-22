using System.Text.Json.Serialization;
using Docker.DotNet.Models;

namespace Frierun.Server.Data;

public record ContainerContract(
    string? Name = null,
    string? ContainerName = null,
    string? ImageName = null,
    bool RequireDocker = false,
    string? NetworkName = null,
    IReadOnlyList<string>? Command = null,
    IReadOnlyDictionary<string, string>? Env = null,
    IEnumerable<Action<CreateContainerParameters>>? Configure = null,
    IEnumerable<Resource>? DependsOn = null
) : Contract<Container>(Name ?? "")
{
    public IReadOnlyList<string> Command { get; init; } = Command ?? Array.Empty<string>();
    public IReadOnlyDictionary<string, string> Env { get; init; } = Env ?? new Dictionary<string, string>();

    [JsonIgnore]
    public IEnumerable<Action<CreateContainerParameters>> Configure { get; init; } =
        Configure ?? Array.Empty<Action<CreateContainerParameters>>();

    [JsonIgnore]
    public IEnumerable<Resource> DependsOn { get; init; } = DependsOn ?? Array.Empty<Resource>();
    
    public string NetworkName { get; init; } = NetworkName ?? "";
    [JsonIgnore]
    public ContractId NetworkId => new(typeof(Network), NetworkName);
}