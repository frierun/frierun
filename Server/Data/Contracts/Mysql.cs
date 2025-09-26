using System.Diagnostics.CodeAnalysis;
using static Frierun.Server.Data.Merger;

namespace Frierun.Server.Data;

public record Mysql(
    ContractRef<Network>? Network = null,
    string? Username = null,
    string? Password = null,
    string? Host = null,
    string? Database = null,
    string? NetworkName = null,
    bool Admin = false
) : Contract
{
    [MemberNotNullWhen(true, nameof(Username), nameof(Password), nameof(Host), nameof(NetworkName))]
    public override bool Installed => Id != null;

    public override Contract Merge(Contract other)
    {
        return MergeCommon(this, other, out var contract) with
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

    public ContractRef<Network> Network { get; init; } = Network ?? new ContractRef<Network>("");
}