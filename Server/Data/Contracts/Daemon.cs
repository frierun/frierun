using System.Diagnostics.CodeAnalysis;
using static Frierun.Server.Data.Merger;

namespace Frierun.Server.Data;

public record Daemon(
    string? Name = null,
    string? DaemonName = null,
    Argument<IEnumerable<string>>? Command = null,
    Argument<IEnumerable<IEnumerable<string>>>? PreCommands = null
) : Contract(Name ?? "")
{
    [MemberNotNullWhen(true, nameof(DaemonName))]
    public override bool Installed { get; init; }

    public Argument<IEnumerable<string>> Command { get; init; } = Command ?? new Argument<IEnumerable<string>>();

    public Argument<IEnumerable<IEnumerable<string>>> PreCommands { get; init; } = PreCommands
        ?? new Argument<IEnumerable<IEnumerable<string>>>();

    public override IEnumerable<IArgument> GetArguments()
    {
        yield return Command;
        yield return PreCommands;
    }

    public override Contract Merge(Contract other)
    {
        var contract = EnsureSame(this, other);

        return MergeCommon(this, other) with
        {
            Command = Command.Merge(contract.Command),
            PreCommands = PreCommands.Merge(contract.PreCommands)
        };
    }
}