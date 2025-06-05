using System.Diagnostics.CodeAnalysis;

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
}