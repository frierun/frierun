using System.Diagnostics.CodeAnalysis;
using static Frierun.Server.Data.Merger;

namespace Frierun.Server.Data;

public record CloudflareTunnel(
    string? AccountId = null,
    string? TunnelName = null,
    string? TunnelId = null,
    string? Token = null,
    ContractId<CloudflareApiConnection>? CloudflareApiConnection = null,
    ContractId<Container>? Container = null
) : Contract
{
    [MemberNotNullWhen(true, nameof(TunnelId), nameof(Token), nameof(AccountId))]
    public override bool Installed => Id != Guid.Empty;

    public ContractId<CloudflareApiConnection> CloudflareApiConnection { get; init; } =
        CloudflareApiConnection ?? new ContractId<CloudflareApiConnection>();

    public ContractId<Container> Container { get; init; } = Container ?? new ContractId<Container>();

    public override IEnumerable<IArgument> GetArguments()
    {
        yield return CloudflareApiConnection;
        yield return Container;
        foreach (var argument in base.GetArguments())
        {
            yield return argument;
        }
    }

    public override Contract Merge(Contract other)
    {
        return MergeCommon(this, other, out var contract) with
        {
            AccountId = MergeValue(AccountId, contract.AccountId),
            CloudflareApiConnection = MergeContractId(CloudflareApiConnection, contract.CloudflareApiConnection),
            Container = MergeContractId(Container, contract.Container),
            TunnelId = MergeValue(TunnelId, contract.TunnelId),
            TunnelName = MergeValue(TunnelName, contract.TunnelName),
            Token = MergeValue(Token, contract.Token)
        };
    }
}