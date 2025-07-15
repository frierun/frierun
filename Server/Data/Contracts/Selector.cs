using System.Diagnostics.CodeAnalysis;
using static Frierun.Server.Data.Merger;

namespace Frierun.Server.Data;

public record SelectorOption(string Name, List<Contract> Contracts);

public record Selector(
    string Name,
    IReadOnlyList<SelectorOption>? Options = null,
    string? Value = null
) : Contract(Name ?? "")
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