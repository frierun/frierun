namespace Frierun.Server.Data;

public class Application : Resource
{
    public required string Name { get; init; }
    public Package? Package { get; init; }
    public string? Url { get; init; }
    public string? Description { get; init; }
    public IReadOnlyList<Resource> Resources { get; init; } = Array.Empty<Resource>();
    public IReadOnlyList<string> RequiredApplications { get; init; } = Array.Empty<string>();
}