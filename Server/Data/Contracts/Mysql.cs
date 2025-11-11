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
    public override bool Installed => Id != Guid.Empty;

    public override Contract Merge(Contract other)
    {
        return MergeCommon(this, other, out var contract) with
        {
            Username = MergeValue(Username, contract.Username),
            Password = MergeValue(Password, contract.Password),
            Host = MergeValue(Host, contract.Host),
            Database = MergeValue(Database, contract.Database),
            NetworkName = MergeValue(NetworkName, contract.NetworkName),
            Network = MergeValue(Network, contract.Network),
            Admin = Admin || contract.Admin
        };
    }

    public ContractRef<Network> Network { get; init; } = Network ?? new ContractRef<Network>("");
}