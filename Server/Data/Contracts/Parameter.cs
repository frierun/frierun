using System.Diagnostics.CodeAnalysis;
using static Frierun.Server.Data.Merger;

namespace Frierun.Server.Data;

public record Parameter(
    string? DefaultValue = null,
    Argument<string>? Value = null 
) : Contract
{
    [MemberNotNullWhen(true, nameof(Value))]
    public override bool Installed => Id != null;

    public Argument<string> Value { get; init; } = Value ?? new Argument<string>();

    public override IEnumerable<IArgument> GetArguments()
    {
        yield return Value;
    }

    public override Contract Merge(Contract other)
    {
        var contract = EnsureSame(this, other);

        return MergeCommon(this, contract) with
        {
            Value = Value.Merge(contract.Value),
            DefaultValue = OnlyOne(DefaultValue, contract.DefaultValue)
        };
    }
}