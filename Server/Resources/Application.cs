namespace Frierun.Server.Resources;

public record Application(
    Guid Id,
    string Name,
    Package? Package = null
): Resource;