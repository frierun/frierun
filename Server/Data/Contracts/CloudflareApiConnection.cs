using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Frierun.Server.Handlers;
using static Frierun.Server.Data.Merger;

namespace Frierun.Server.Data;

public record CloudflareApiConnection(
    string? Token = null
) : Contract<ICloudflareApiConnectionHandler>
{
    [MemberNotNullWhen(true, nameof(Token))]
    public override bool Installed => Id != Guid.Empty;

    /// <summary>
    /// Create a cloudflare client from the contract.
    /// </summary>
    public ICloudflareClient CreateClient()
    {
        Debug.Assert(Handler != null);
        return Handler.CreateClient(this);
    }

    public override Contract Merge(Contract other)
    {
        return MergeCommon(this, other, out var contract) with
        {
            Token = MergeValue(Token, contract.Token)
        };
    }
}