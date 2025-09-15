using static Frierun.Server.Data.Merger;

namespace Frierun.Server.Data;

public record Package(
    string Name,
    string? Url = null,
    string? Prefix = null,
    string? IconUrl = null,
    Argument<string>? ApplicationUrl = null,
    Argument<string>? ApplicationDescription = null,
    string? ShortDescription = null,
    string? FullDescription = null,
    IReadOnlyList<string>? Tags = null,
    ContractList? Contracts = null,
    Application? Result = null
)
{
    public IReadOnlyList<string> Tags { get; init; } = Tags ?? [];
    public ContractList Contracts { get; init; } = Contracts ?? [];
    public Argument<string> ApplicationUrl { get; init; } = ApplicationUrl ?? new Argument<string>();
    public Argument<string> ApplicationDescription { get; init; } = ApplicationDescription ?? new Argument<string>();
    
    public Application CreateApplication(string? name = null, ContractList? contracts = null)
    {
        return new Application(Name)
        {
            Prefix = name,
            Package = this,
            Description = ApplicationDescription,
            Url = ApplicationUrl,
            Contracts = Contracts.Merge(contracts ?? []),
        };
    }
}