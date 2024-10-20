namespace Frierun.Server.Resources;

public record Package(
    string Name,
    string Url,
    IReadOnlyList<ResourceDefinition> Children
) : ResourceDefinition<Application>(Children);