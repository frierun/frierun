namespace Frierun.Server.Data;

public record Volume(
    string Name,
    string? VolumeName = null
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
            VolumeName = volume.VolumeName ?? VolumeName,
        };
    }
}