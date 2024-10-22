namespace Frierun.Server.Resources;

public record FileDefinition(
    string Path,
    string Text
) : ResourceDefinition<File>(new List<ResourceDefinition>());