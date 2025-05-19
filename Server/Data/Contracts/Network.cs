using System.Diagnostics.CodeAnalysis;

namespace Frierun.Server.Data;

public record Network(
    string Name,
    string? NetworkName = null
) : Contract(Name)
{
    [MemberNotNullWhen(true, nameof(NetworkName))]
    public override bool Installed { get; init; }
}
