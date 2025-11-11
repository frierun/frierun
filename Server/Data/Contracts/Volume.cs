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
            VolumeName = MergeValue(VolumeName, contract.VolumeName),
            LocalPath = MergeValue(LocalPath, contract.LocalPath)
        };
    }

    public override bool IsSubset(Contract other)
    {
        return IsSubsetContract(this, other, out var contract)
               && IsSubsetValue(VolumeName, contract.VolumeName)
               && IsSubsetValue(LocalPath, contract.LocalPath);
    }
}