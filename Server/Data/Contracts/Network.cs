namespace Frierun.Server.Data;

public record Network(
    string Name,
    string? NetworkName = null,
    DockerNetwork? Result = null
) : Contract(Name), IHasResult<DockerNetwork>;
