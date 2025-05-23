using System.Diagnostics.CodeAnalysis;

namespace Frierun.Server.Data;

public record Password(
    string? Name = null,
    string? Value = null
) : Contract(Name ?? "")
{
    [MemberNotNullWhen(true, nameof(Value))]
    public override bool Installed { get; init; }    
}