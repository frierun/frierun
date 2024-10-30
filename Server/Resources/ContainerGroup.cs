namespace Frierun.Server.Resources;

public record ContainerGroup(
    Guid Id,
    string Name,
    IReadOnlyList<Resource> Children
) : Resource(Id, Children);