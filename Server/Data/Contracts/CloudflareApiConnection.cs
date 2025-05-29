namespace Frierun.Server.Data;

public record CloudflareApiConnection(
    string? Name = null,
    string? Token = null
) : Contract(Name ?? ""), IHasStrings
{
    public string Token { get; init; } = Token ?? "";
    
    public Contract ApplyStringDecorator(Func<string, string> decorator)
    {
        return this with
        {
            Token = decorator(Token)
        };
    }

}