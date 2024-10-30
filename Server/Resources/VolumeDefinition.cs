namespace Frierun.Server.Resources;

public record VolumeDefinition(
    string Path,
    IReadOnlyList<ResourceDefinition> Children,
    string? Name = null,
    bool ReadOnly = false
) : ResourceDefinition<Volume>(Children, Name);