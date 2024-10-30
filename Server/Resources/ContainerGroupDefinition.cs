namespace Frierun.Server.Resources;

public record ContainerGroupDefinition(
    IReadOnlyList<ResourceDefinition> Children,
    string? Name = null
) : ResourceDefinition<ContainerGroup>(Children, Name);
