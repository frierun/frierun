namespace Frierun.Server.Data;

public record Volume(
    string Name,
    string? VolumeName = null
) : Contract(Name);