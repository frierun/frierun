namespace Frierun.Server.Data;

public record VolumeContract(
    string Name,
    string? VolumeName = null
) : Contract(Name);