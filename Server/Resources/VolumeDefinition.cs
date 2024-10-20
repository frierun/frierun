namespace Frierun.Server.Resources;

public record VolumeDefinition(string Name, string Path) : ResourceDefinition<Volume>(new List<ResourceDefinition>());