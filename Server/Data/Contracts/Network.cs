namespace Frierun.Server.Data;

public record Network(
    string Name,
    string? NetworkName = null
) : Contract(Name);
