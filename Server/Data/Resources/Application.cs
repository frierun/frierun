namespace Frierun.Server.Data;

public record Application(
    string Name,
    Package? Package = null
): Resource;