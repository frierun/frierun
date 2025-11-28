using System.Diagnostics.CodeAnalysis;
using static Frierun.Server.Data.Merger;

namespace Frierun.Server.Data;

public record Parameter(
    string? DefaultValue = null,
    Argument<string>? Value = null 
) : Contract
{
    [MemberNotNullWhen(true, nameof(Value))]
    public override bool Installed => Id != Guid.Empty;

    public Argument<string> Value { get; init; } = Value ?? new Argument<string>();

    public override Parameter Transform(IArgumentTransformer transformer)
    {
        return this with
        {
            Value = transformer.Transform(Value),
            DependsOn = DependsOn.Select(transformer.Transform).ToArray()
        };
    }
    
    public override Contract Merge(Contract other)
    {
        return MergeCommon(this, other, out var contract) with
        {
            Value = Value.Merge(contract.Value),
            DefaultValue = MergeValue(DefaultValue, contract.DefaultValue)
        };
    }
}