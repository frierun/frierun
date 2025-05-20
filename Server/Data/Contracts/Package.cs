namespace Frierun.Server.Data;

public record Package(
    string Name,
    string? Url = null,
    string? Prefix = null,
    string? IconUrl = null,
    string? ApplicationUrl = null,
    string? ApplicationDescription = null,
    string? ShortDescription = null,
    string? FullDescription = null,
    IReadOnlyList<string>? Tags = null,
    IEnumerable<Contract>? Contracts = null,
    Application? Result = null
) : Contract(Name), IHasStrings
{
    public IReadOnlyList<string> Tags { get; init; } = Tags ?? Array.Empty<string>();
    public IEnumerable<Contract> Contracts { get; init; } = Contracts ?? Array.Empty<Contract>();

    public Contract ApplyStringDecorator(Func<string, string> decorator)
    {
        return this with
        {
            ApplicationUrl = ApplicationUrl != null ? decorator(ApplicationUrl) : null,
            ApplicationDescription = ApplicationDescription != null ? decorator(ApplicationDescription) : null,
        };
    }

    public override Contract With(Contract other)
    {
        if (other is not Package package || other.Id != Id)
        {
            throw new Exception("Invalid contract");
        }
        
        return this with
        {
            Prefix = package.Prefix ?? Prefix,
            Contracts = Contracts.Concat(package.Contracts)
                .GroupBy(contract => contract.Id)
                .Select(group => group.Aggregate((a, b) => a.With(b)))
        };
    }
}