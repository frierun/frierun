namespace Frierun.Server.Data;

public class ResolvedDomain : Resource
{
    public required string Value { get; init; } 
    public required bool IsInternal { get; init; }
}