using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Frierun.Server.Handlers;
using static Frierun.Server.Data.Merger;

namespace Frierun.Server.Data;

public record CloudflareApiConnection(
    string? Name = null,
    string? Token = null
) : Contract<ICloudflareApiConnectionHandler>(Name ?? "")
{
    [MemberNotNullWhen(true, nameof(Token))]
    public override bool Installed { get; init; }
    
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
        var contract = EnsureSame(this, other);

        return MergeCommon(this, other) with
        {
            Token = OnlyOne(Token, contract.Token)
        };
    }
}