using System.Diagnostics.CodeAnalysis;
using static Frierun.Server.Data.Merger;

namespace Frierun.Server.Data;

public record Password(
    string? Value = null
) : Contract
{
    [MemberNotNullWhen(true, nameof(Value))]
    public override bool Installed => Id != null;

    public override Contract Merge(Contract other)
    {
        return MergeCommon(this, other, out var contract) with
        {
            Value = OnlyOne(Value, contract.Value)
        };
    }
}