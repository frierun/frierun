namespace Frierun.Server.Data;

public record Package(
    string Name,
    string? Url = null,
    string? Prefix = null,
    string? ApplicationUrl = null,
    string? ApplicationDescription = null,
    IEnumerable<Contract>? Contracts = null
) : Contract(Name), IHasStrings
{
    public IEnumerable<Contract> Contracts { get; init; } = Contracts ?? Array.Empty<Contract>();

    /// <inheritdoc />
    public Contract ApplyStringDecorator(Func<string, string> decorator)
    {
        return this with
        {
            ApplicationUrl = ApplicationUrl != null ? decorator(ApplicationUrl) : null,
            ApplicationDescription = ApplicationDescription != null ? decorator(ApplicationDescription) : null,
        };
    }

    /// <inheritdoc />
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