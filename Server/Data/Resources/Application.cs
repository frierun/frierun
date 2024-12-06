namespace Frierun.Server.Data;

public record Application(
    string Name,
    Package? Package = null,
    string? Url = null,
    string? Description = null
): Resource;