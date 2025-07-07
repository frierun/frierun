using System.Diagnostics.CodeAnalysis;
using static Frierun.Server.Data.Merger;

namespace Frierun.Server.Data;

public record Domain(
    string? Name = null,
    string? Value = null,
    bool? IsInternal = null
) : Contract(Name ?? "")
{
    [MemberNotNullWhen(true, nameof(Value), nameof(IsInternal))]
    public override bool Installed { get; init; }
    
    public override Contract Merge(Contract other)
    {
        var contract = EnsureSame(this, other);

        return MergeCommon(this, other) with
        {
            Value = OnlyOne(Value, contract.Value),
            IsInternal = OnlyOne(IsInternal, contract.IsInternal)       
        };
    }    
}