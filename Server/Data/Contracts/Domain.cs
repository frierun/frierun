namespace Frierun.Server.Data;

public record Domain(
    string? Name = null,
    string? Subdomain = null
) : Contract(Name ?? "")
{
}