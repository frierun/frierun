namespace Frierun.Server.Resources;

public record VolumeDefinition(
    string Name,
    string Path,
    bool ReadOnly = false
) : ResourceDefinition<Volume>(new List<ResourceDefinition>());