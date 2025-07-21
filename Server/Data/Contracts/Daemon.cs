using System.Diagnostics.CodeAnalysis;
using static Frierun.Server.Data.Merger;

namespace Frierun.Server.Data;

public record Daemon(
    string? Name = null,
    string? DaemonName = null,
    IReadOnlyList<string>? Command = null,
    IReadOnlyList<IReadOnlyList<string>>? PreCommands = null
) : Contract(Name ?? "")
{
    [MemberNotNullWhen(true, nameof(DaemonName))]
    public override bool Installed { get; init; }
    
    public IReadOnlyList<string> Command { get; init; } = Command ?? [];
    public IReadOnlyList<IReadOnlyList<string>> PreCommands { get; init; } = PreCommands ?? [];
    
    public override Contract Merge(Contract other)
    {
        var contract = EnsureSame(this, other);

        return MergeCommon(this, other) with
        {
            Command = OnlyOne(Command, contract.Command, command => command.Count == 0),
            PreCommands = OnlyOne(PreCommands, contract.PreCommands, command => command.Count == 0)
        };
    }
}