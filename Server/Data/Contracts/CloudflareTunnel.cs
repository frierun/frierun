using System.Diagnostics.CodeAnalysis;
using static Frierun.Server.Data.Merger;

namespace Frierun.Server.Data;

public record CloudflareTunnel(
    string? Name = null,
    string? AccountId = null,
    string? TunnelName = null,
    string? TunnelId = null,
    string? Token = null,
    ContractId<CloudflareApiConnection>? CloudflareApiConnection = null,
    ContractId<Container>? Container = null
) : Contract(Name ?? "")
{
    [MemberNotNullWhen(true, nameof(TunnelId), nameof(Token), nameof(AccountId))]
    public override bool Installed { get; init; }

    public ContractId<CloudflareApiConnection> CloudflareApiConnection { get; init; } =
        CloudflareApiConnection ?? new ContractId<CloudflareApiConnection>("");

    public ContractId<Container> Container { get; init; } = Container ?? new ContractId<Container>("");
    
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