namespace Frierun.Server.Data;

public record Application(
    string Name,
    Package? Package = null,
    string? Url = null,
    string? Description = null
) : Resource
{
    public IReadOnlyList<Resource> Resources { get; init; } = Array.Empty<Resource>();
    public IReadOnlyList<string> RequiredApplications { get; init; } = Array.Empty<string>();
}