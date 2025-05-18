using System.Text.Json.Serialization;

namespace Frierun.Server.Data;


public class Application : Resource
{
    public required string Name { get; init; }
    public Package? Package { get; init; }
    public string? Url { get; init; }
    public string? Description { get; init; }
    public IReadOnlyList<Contract> Contracts { get; init; } = Array.Empty<Contract>();
    public IReadOnlyList<string> RequiredApplications { get; init; } = Array.Empty<string>();
}