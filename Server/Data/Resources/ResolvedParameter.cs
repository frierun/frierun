namespace Frierun.Server.Data;

public class ResolvedParameter : Resource
{
    public required string Name { get; init; }
    public string? Value { get; init; }
}