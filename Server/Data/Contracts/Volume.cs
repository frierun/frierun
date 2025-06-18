namespace Frierun.Server.Data;

public record Volume(
    string Name,
    string? VolumeName = null,
    string? LocalPath = null
) : Contract(Name)
{
    public override Contract With(Contract other) 
    {
        if (other is not Volume volume || other.Id != Id)
        {
            throw new Exception("Invalid contract");
        }

        return this with
        {
            VolumeName = VolumeName ?? volume.VolumeName,
            LocalPath = LocalPath ?? volume.LocalPath
        };
    }
}