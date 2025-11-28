using System.Diagnostics.CodeAnalysis;
using static Frierun.Server.Data.Merger;

namespace Frierun.Server.Data;

public record SelectorOption(string Name, ContractList? Contracts);

public record Selector(
    IReadOnlyList<SelectorOption>? Options = null,
    string? Value = null
) : Contract
{
    [MemberNotNullWhen(true, nameof(Value))]
    public override bool Installed => Id != Guid.Empty;
    
    public IReadOnlyList<SelectorOption> Options { get; init; } = Options ?? [];
    
    public override Contract Merge(Contract other)
    {
        return MergeCommon(this, other, out var contract) with
        {
            Value = MergeValue(Value, contract.Value),
        };
    }
}