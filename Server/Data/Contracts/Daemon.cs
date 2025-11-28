using System.Diagnostics.CodeAnalysis;
using static Frierun.Server.Data.Merger;

namespace Frierun.Server.Data;

public record Daemon(
    string? DaemonName = null,
    Argument<IEnumerable<string>>? Command = null,
    Argument<IEnumerable<IEnumerable<string>>>? PreCommands = null
) : Contract
{
    [MemberNotNullWhen(true, nameof(DaemonName))]
    public override bool Installed => Id != Guid.Empty;

    public Argument<IEnumerable<string>> Command { get; init; } = Command ?? new Argument<IEnumerable<string>>();

    public Argument<IEnumerable<IEnumerable<string>>> PreCommands { get; init; } = PreCommands
        ?? new Argument<IEnumerable<IEnumerable<string>>>();

    public override Daemon Transform(IArgumentTransformer transformer)
    {
        return this with
        {
            Command = transformer.Transform(Command),
            PreCommands = transformer.Transform(PreCommands),
            DependsOn = DependsOn.Select(transformer.Transform).ToArray()
        };
    }
    
    public override Contract Merge(Contract other)
    {
        return MergeCommon(this, other, out var contract) with
        {
            DaemonName = MergeValue(DaemonName, contract.DaemonName),
            Command = Command.Merge(contract.Command),
            PreCommands = PreCommands.Merge(contract.PreCommands)
        };
    }
}