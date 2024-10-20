namespace Frierun.Server.Resources;

public record Application(
    Guid Id,
    string Name,
    IReadOnlyList<Resource> Children,
    Package? Package = null
): Resource(Id, Children);