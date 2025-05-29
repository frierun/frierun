namespace Frierun.Server.Data;

public record CloudflareApiConnection(
    string? Name = null,
    string? ApiKey = null
) : Contract(Name ?? ""), IHasStrings
{
    public Contract ApplyStringDecorator(Func<string, string> decorator)
    {
        return this with
        {
            ApiKey = ApiKey != null ? decorator(ApiKey) : null
        };
    }
}