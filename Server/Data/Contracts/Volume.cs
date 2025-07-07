using static Frierun.Server.Data.Merger;

namespace Frierun.Server.Data;

public record Volume(
    string Name,
    string? VolumeName = null,
    string? LocalPath = null
) : Contract(Name)
{
    public override Contract Merge(Contract other) 
    {
        var contract = EnsureSame(this, other);
        
        return MergeCommon(this, contract) with
        {
            VolumeName = OnlyOne(VolumeName, contract.VolumeName),
            LocalPath = OnlyOne(LocalPath, contract.LocalPath)
        };
    }
}