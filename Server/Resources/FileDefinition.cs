namespace Frierun.Server.Resources;

public record FileDefinition(
    string Path,
    string Text,
    string? Name = null
) : ResourceDefinition<File>(new List<ResourceDefinition>(), Name);