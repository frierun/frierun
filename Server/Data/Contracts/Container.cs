using System.Text.Json.Serialization;
using Docker.DotNet.Models;

namespace Frierun.Server.Data;

public record Container(
    string? Name = null,
    string? ContainerName = null,
    string? ImageName = null,
    bool RequireDocker = false,
    string? NetworkName = null,
    IReadOnlyList<string>? Command = null,
    IReadOnlyDictionary<string, string>? Env = null,
    IEnumerable<Action<CreateContainerParameters>>? Configure = null,
    DockerContainer? Result = null
) : Contract(Name ?? ""), IHasStrings, IHasResult<DockerContainer>
{
    public IReadOnlyList<string> Command { get; init; } = Command ?? Array.Empty<string>();
    public IReadOnlyDictionary<string, string> Env { get; init; } = Env ?? new Dictionary<string, string>();

    [JsonIgnore]
    public IEnumerable<Action<CreateContainerParameters>> Configure { get; init; } =
        Configure ?? Array.Empty<Action<CreateContainerParameters>>();


    public string NetworkName { get; init; } = NetworkName ?? "";
    [JsonIgnore] public ContractId<Network> NetworkId => new(NetworkName);
    
    Contract IHasStrings.ApplyStringDecorator(Func<string, string> decorator)
    {
        return this with
        {
            Command = Command.Select(decorator).ToList(),
            Env = Env.ToDictionary(kv => decorator(kv.Key), kv => decorator(kv.Value)),
        };
    }
}