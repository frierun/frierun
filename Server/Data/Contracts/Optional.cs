using System.Diagnostics.CodeAnalysis;
using static Frierun.Server.Data.Merger;

namespace Frierun.Server.Data;

public record Optional(
    string Name,
    IReadOnlyList<Contract>? Contracts = null,
    bool? Value = null
) : Contract(Name ?? "")
{
    [MemberNotNullWhen(true, nameof(Value))]
    public override bool Installed { get; init; }
    
    public IReadOnlyList<Contract> Contracts { get; init; } = Contracts ?? [];
    
    public override Contract Merge(Contract other)
    {
        var contract = EnsureSame(this, other);

        return MergeCommon(this, contract) with
        {
            Value = OnlyOne(Value, contract.Value),
        };
    }
}