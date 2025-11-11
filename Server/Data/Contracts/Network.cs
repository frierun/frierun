using System.Diagnostics.CodeAnalysis;
using static Frierun.Server.Data.Merger;

namespace Frierun.Server.Data;

public record Network(
    string? NetworkName = null
) : Contract
{
    [MemberNotNullWhen(true, nameof(NetworkName))]
    public override bool Installed => Id != Guid.Empty;

    public override Contract Merge(Contract other)
    {
        return MergeCommon(this, other, out var contract) with
        {
            NetworkName = MergeValue(NetworkName, contract.NetworkName)
        };
    }
}
