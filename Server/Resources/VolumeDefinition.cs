namespace Frierun.Server.Resources;

public record VolumeDefinition(
    string Name,
    string Path,
    IReadOnlyList<ResourceDefinition> Children,
    bool ReadOnly = false
) : ResourceDefinition<Volume>(Children);