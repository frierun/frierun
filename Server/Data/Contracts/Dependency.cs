using static Frierun.Server.Data.Merger;

namespace Frierun.Server.Data;

public record Dependency(
    ContractId Preceding,
    ContractId Following
) : Contract($"{Preceding} -> {Following}")
{
    public override Contract Merge(Contract other)
    {
        var contract = EnsureSame(this, other);

        return MergeCommon(this, other) with
        {
            Preceding = OnlyOne(Preceding, contract.Preceding),
            Following = OnlyOne(Following, contract.Following)       
        };
    }
}