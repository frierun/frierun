using System.Diagnostics.CodeAnalysis;
using static Frierun.Server.Data.Merger;

namespace Frierun.Server.Data;

public record Network(
    string Name,
    string? NetworkName = null
) : Contract(Name)
{
    [MemberNotNullWhen(true, nameof(NetworkName))]
    public override bool Installed { get; init; }

    public override Contract Merge(Contract other)
    {
        var contract = EnsureSame(this, other);

        return MergeCommon(this, contract) with
        {
            NetworkName = OnlyOne(NetworkName, contract.NetworkName)
        };
    }
}
