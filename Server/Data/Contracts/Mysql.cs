using System.Diagnostics.CodeAnalysis;

namespace Frierun.Server.Data;

public record Mysql(
    string? Name = null,
    ContractId<Network>? Network = null,
    string? Username = null,
    string? Password = null,
    string? Host = null,
    string? Database = null,
    string? NetworkName = null,
    bool Admin = false
) : Contract(Name ?? "")
{
    [MemberNotNullWhen(true, nameof(Username), nameof(Password), nameof(Host), nameof(NetworkName))]
    public override bool Installed { get; init; }
    
    public ContractId<Network> Network { get; init; } = Network ?? new ContractId<Network>("");
}