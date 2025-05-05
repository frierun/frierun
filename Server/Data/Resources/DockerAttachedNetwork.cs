namespace Frierun.Server.Data;

public class DockerAttachedNetwork(
) : Resource
{
    public required string ContainerName { get; init; }
    public required  string NetworkName { get; init; }
}
