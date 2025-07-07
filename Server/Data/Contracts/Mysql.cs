using System.Diagnostics.CodeAnalysis;
using static Frierun.Server.Data.Merger;

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

    public override Contract Merge(Contract other)
    {
        var contract = EnsureSame(this, other);
    
        return MergeCommon(this, contract) with
        {
            Username = OnlyOne(Username, contract.Username),
            Password = OnlyOne(Password, contract.Password),
            Host = OnlyOne(Host, contract.Host),
            Database = OnlyOne(Database, contract.Database),
            NetworkName = OnlyOne(NetworkName, contract.NetworkName),
            Network = OnlyOne(Network, contract.Network),
            Admin = Admin || contract.Admin
        };
    }

    public ContractId<Network> Network { get; init; } = Network ?? new ContractId<Network>("");
}