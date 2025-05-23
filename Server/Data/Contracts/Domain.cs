using System.Diagnostics.CodeAnalysis;

namespace Frierun.Server.Data;

public record Domain(
    string? Name = null,
    string? Value = null,
    bool? IsInternal = null
) : Contract(Name ?? "")
{
    [MemberNotNullWhen(true, nameof(Value), nameof(IsInternal))]
    public override bool Installed { get; init; }    
}