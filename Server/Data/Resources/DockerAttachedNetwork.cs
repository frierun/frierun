namespace Frierun.Server.Data;

public record DockerAttachedNetwork(
    string ContainerName,
    string NetworkName
) : Resource;