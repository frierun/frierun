using System.Diagnostics.CodeAnalysis;
using static Frierun.Server.Data.Merger;

namespace Frierun.Server.Data;

public record CloudflareTunnel(
    string? AccountId = null,
    string? TunnelName = null,
    string? TunnelId = null,
    string? Token = null,
    ContractId<CloudflareApiConnection>? CloudflareApiConnection = null,
    ContractRef<Container>? Container = null
) : Contract
{
    [MemberNotNullWhen(true, nameof(TunnelId), nameof(Token), nameof(AccountId))]
    public override bool Installed => Id != Guid.Empty;

    public ContractId<CloudflareApiConnection> CloudflareApiConnection { get; init; } =
        CloudflareApiConnection ?? new ContractId<CloudflareApiConnection>();

    public override CloudflareTunnel Transform(IArgumentTransformer transformer)
    {
        return this with
        {
            CloudflareApiConnection = transformer.Transform(CloudflareApiConnection),
            DependsOn = DependsOn.Select(transformer.Transform).ToArray()
        };
    }
    
    public override Contract Merge(Contract other)
    {
        return MergeCommon(this, other, out var contract) with
        {
            AccountId = MergeValue(AccountId, contract.AccountId),
            CloudflareApiConnection = MergeValue(CloudflareApiConnection, contract.CloudflareApiConnection),
            Container = MergeValue(Container, contract.Container),
            TunnelId = MergeValue(TunnelId, contract.TunnelId),
            TunnelName = MergeValue(TunnelName, contract.TunnelName),
            Token = MergeValue(Token, contract.Token)
        };
    }
}