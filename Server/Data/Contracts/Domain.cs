namespace Frierun.Server.Data;

public record Domain(
    string? Name = null,
    string? Subdomain = null,
    ResolvedDomain? Result = null
) : Contract(Name ?? ""), IHasResult<ResolvedDomain>
{
}