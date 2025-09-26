using System.Diagnostics.CodeAnalysis;
using static Frierun.Server.Data.Merger;

namespace Frierun.Server.Data;

public record Optional(
    ContractList? Contracts = null,
    bool? Value = null
) : Contract
{
    [MemberNotNullWhen(true, nameof(Value))]
    public override bool Installed => Id != null;
    
    public ContractList Contracts { get; init; } = Contracts ?? [];
    
    public override Contract Merge(Contract other)
    {
        return MergeCommon(this, other, out var contract) with
        {
            Value = OnlyOne(Value, contract.Value),
        };
    }
}