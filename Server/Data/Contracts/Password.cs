namespace Frierun.Server.Data;

public record Password(
    string? Name = ""
) : Contract(Name ?? "");