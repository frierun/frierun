using System.Diagnostics.CodeAnalysis;
using static Frierun.Server.Data.Merger;

namespace Frierun.Server.Data;

public record Password(
    string? Value = null
) : Contract
{
    [MemberNotNullWhen(true, nameof(Value))]
    public override bool Installed { get; init; }

    public override Contract Merge(Contract other)
    {
        var contract = EnsureSame(this, other);

        return MergeCommon(this, contract) with
        {
            Value = OnlyOne(Value, contract.Value)
        };
    }
}