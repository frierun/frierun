using System.Diagnostics.CodeAnalysis;
using static Frierun.Server.Data.Merger;

namespace Frierun.Server.Data;

public record Postgresql(
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

    public override Contract Merge(Contract other)
    {
        var contract = EnsureSame(this, other);

        return MergeCommon(this, contract) with
        {
            Network = OnlyOne(Network, contract.Network),
            Username = OnlyOne(Username, contract.Username),
            Password = OnlyOne(Password, contract.Password),
            Host = OnlyOne(Host, contract.Host),
            NetworkName = OnlyOne(NetworkName, contract.NetworkName),
            Admin = Admin || contract.Admin
        };
    }
}