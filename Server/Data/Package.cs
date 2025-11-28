namespace Frierun.Server.Data;

public record Package(
    string Name,
    string? Url = null,
    string? IconUrl = null,
    string? ApplicationUrl = null,
    string? ApplicationDescription = null,
    string? ShortDescription = null,
    string? FullDescription = null,
    IReadOnlyList<string>? Tags = null,
    ContractList? Contracts = null,
    Application? Result = null
)
{
    public IReadOnlyList<string> Tags { get; init; } = Tags ?? [];
    public ContractList Contracts { get; init; } = Contracts ?? [];
    
    public Application CreateApplication(string? name = null, ContractList? contracts = null)
    {
        return new Application(name ?? "")
        {
            Package = this with
            {
                Contracts = Contracts.Merge(contracts ?? [])
            },
            Description = new Argument<string>(ApplicationDescription),
            Url = new Argument<string>(ApplicationUrl),
        };
    }
}