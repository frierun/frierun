namespace Frierun.Server.Data;

public record Package(
    string Name,
    string? Prefix = null,
    string? Url = null,
    IEnumerable<Contract>? Contracts = null
) : Contract(Name)
{
    public IEnumerable<Contract> Contracts { get; init; } = Contracts ?? Array.Empty<Contract>();

    /// <inheritdoc />
    public override Contract With(Contract other)
    {
        if (other.Id != Id)
        {
            throw new Exception("Invalid contract id");
        }
        
        if (other is not Package package)
        {
            throw new Exception("Invalid contract type");
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