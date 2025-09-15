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
    public override bool Installed { get; init; }
    
    public IReadOnlyList<SelectorOption> Options { get; init; } = Options ?? [];
    
    public override Contract Merge(Contract other)
    {
        var contract = EnsureSame(this, other);

        return MergeCommon(this, contract) with
        {
            Value = OnlyOne(Value, contract.Value),
        };
    }
}