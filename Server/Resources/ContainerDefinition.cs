namespace Frierun.Server.Resources;

public record ContainerDefinition(
    string ImageName,
    string? Command = null,
    bool RequireDocker = false
) : ResourceDefinition<Container>;
