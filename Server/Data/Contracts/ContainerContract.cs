using Docker.DotNet.Models;

namespace Frierun.Server.Data;

public record ContainerContract(
    string? Name = null,
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

    public IEnumerable<Action<CreateContainerParameters>> Configure { get; init; } =
        Configure ?? Array.Empty<Action<CreateContainerParameters>>();

    public IEnumerable<Resource> DependsOn { get; init; } = DependsOn ?? Array.Empty<Resource>();
    
    public string NetworkName { get; init; } = NetworkName ?? "";
    public ContractId NetworkId => new(typeof(Network), NetworkName);
}