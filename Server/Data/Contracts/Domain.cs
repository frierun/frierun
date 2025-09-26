using System.Diagnostics.CodeAnalysis;
using static Frierun.Server.Data.Merger;

namespace Frierun.Server.Data;

public record Domain(
    string? Value = null,
    bool? IsInternal = null
) : Contract
{
    [MemberNotNullWhen(true, nameof(Value), nameof(IsInternal))]
    public override bool Installed => Id != null;
    
    public override Contract Merge(Contract other)
    {
        return MergeCommon(this, other, out var contract) with
        {
            Value = OnlyOne(Value, contract.Value),
            IsInternal = OnlyOne(IsInternal, contract.IsInternal)       
        };
    }    
}