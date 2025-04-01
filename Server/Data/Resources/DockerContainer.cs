namespace Frierun.Server.Data;

public record DockerContainer(
    string Name,
    string NetworkName
) : Resource;