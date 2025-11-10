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

    public override bool IsFulfilling(Contract other)
    {
        if (other is not Volume contract)
        {
            return false;
        }
        
        if (other.HandlerApplication != null && other.HandlerApplication != Handler?.Application?.Name)
        {
            return false;
        }

        if (contract.VolumeName != null && contract.VolumeName != VolumeName)
        {
            return false;
        }

        if (contract.LocalPath != null)
        {
            return false;
        }

        return true;
    }
}