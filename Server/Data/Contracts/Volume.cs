using static Frierun.Server.Data.Merger;

namespace Frierun.Server.Data;

public record Volume(
    string? VolumeName = null,
    string? LocalPath = null
) : Contract
{
    public override Contract Merge(Contract other) 
    {
        return MergeCommon(this, other, out var contract) with
        {
            VolumeName = OnlyOne(VolumeName, contract.VolumeName),
            LocalPath = OnlyOne(LocalPath, contract.LocalPath)
        };
    }
}