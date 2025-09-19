using System.Diagnostics.CodeAnalysis;
using static Frierun.Server.Data.Merger;

namespace Frierun.Server.Data;

public record CloudflareTunnel(
    string? AccountId = null,
    string? TunnelName = null,
    string? TunnelId = null,
    string? Token = null,
    ContractRef<CloudflareApiConnection>? CloudflareApiConnection = null,
    ContractRef<Container>? Container = null
) : Contract
{
    [MemberNotNullWhen(true, nameof(TunnelId), nameof(Token), nameof(AccountId))]
    public override bool Installed => Id != null;

    public ContractRef<CloudflareApiConnection> CloudflareApiConnection { get; init; } =
        CloudflareApiConnection ?? new ContractRef<CloudflareApiConnection>("");

    public ContractRef<Container> Container { get; init; } = Container ?? new ContractRef<Container>("");
    
    public override Contract Merge(Contract other)
    {
        var contract = EnsureSame(this, other);

        return MergeCommon(this, other) with
        {
            AccountId = OnlyOne(AccountId, contract.AccountId),
            CloudflareApiConnection = OnlyOne(CloudflareApiConnection, contract.CloudflareApiConnection),
            Container = OnlyOne(Container, contract.Container),
            TunnelId = OnlyOne(TunnelId, contract.TunnelId),
            TunnelName = OnlyOne(TunnelName, contract.TunnelName),
            Token = OnlyOne(Token, contract.Token)
        };
    }
}