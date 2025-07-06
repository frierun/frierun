using System.Diagnostics;
using Frierun.Server.Handlers;

namespace Frierun.Server.Data;

public record CloudflareApiConnection(
    string? Name = null,
    string? Token = null
) : Contract<ICloudflareApiConnectionHandler>(Name ?? ""), IHasStrings
{
    public string Token { get; init; } = Token ?? "";
    
    public Contract ApplyStringDecorator(Func<string, string> decorator)
    {
        return this with
        {
            Token = decorator(Token)
        };
    }

    /// <summary>
    /// Create a cloudflare client from the contract.
    /// </summary>
    public ICloudflareClient CreateClient()
    {
        Debug.Assert(Handler != null);
        return Handler.CreateClient(this);
    }
}