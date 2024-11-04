namespace Frierun.Server.Resources;

public record Application(
    string Name,
    Package? Package = null
): Resource;