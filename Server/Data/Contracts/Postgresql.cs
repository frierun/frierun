using System.Diagnostics.CodeAnalysis;
using static Frierun.Server.Data.Merger;

namespace Frierun.Server.Data;

public record Postgresql(
    ContractId<Network>? Network = null,
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
    
    public ContractId<Network> Network { get; init; } = Network ?? new ContractId<Network>();
    
    public override Postgresql Transform(IArgumentTransformer transformer)
    {
        return this with
        {
            Network = transformer.Transform(Network),
            DependsOn = DependsOn.Select(transformer.Transform).ToArray()
        };
    }

    public override Contract Merge(Contract other)
    {
        return MergeCommon(this, other, out var contract) with
        {
            Network = MergeValue(Network, contract.Network),
            Username = MergeValue(Username, contract.Username),
            Password = MergeValue(Password, contract.Password),
            Host = MergeValue(Host, contract.Host),
            Database = MergeValue(Database, contract.Database),
            NetworkName = MergeValue(NetworkName, contract.NetworkName),
            Admin = Admin || contract.Admin
        };
    }
}