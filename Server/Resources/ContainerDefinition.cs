namespace Frierun.Server.Resources;

public record ContainerDefinition(
    string ImageName,
    IReadOnlyList<ResourceDefinition> Children,
    string? Name = null,
    IReadOnlyList<string>? Command = null,
    bool RequireDocker = false,
    IReadOnlyDictionary<string, string>? Env = null
) : ResourceDefinition<Container>(Children, Name)
{
    public IReadOnlyList<string> Command { get; } = Command ?? new List<string>();
    public IReadOnlyDictionary<string, string> Env { get; } = Env ?? new Dictionary<string, string>();
}