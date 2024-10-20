namespace Frierun.Server.Resources;

public record ContainerDefinition(
    string ImageName,
    IReadOnlyList<ResourceDefinition> Children,
    string? Command = null,
    bool RequireDocker = false
) : ResourceDefinition<Container>(Children);
