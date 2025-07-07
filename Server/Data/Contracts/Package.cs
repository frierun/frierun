using static Frierun.Server.Data.Merger;

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
    public IReadOnlyList<string> Tags { get; init; } = Tags ?? [];
    public IEnumerable<Contract> Contracts { get; init; } = Contracts ?? [];

    public Contract ApplyStringDecorator(Func<string, string> decorator)
    {
        return this with
        {
            ApplicationUrl = ApplicationUrl != null ? decorator(ApplicationUrl) : null,
            ApplicationDescription = ApplicationDescription != null ? decorator(ApplicationDescription) : null,
        };
    }

    public override Contract Merge(Contract other)
    {
        var contract = EnsureSame(this, other);

        return MergeCommon(this, contract) with
        {
            Prefix = OnlyOne(Prefix, contract.Prefix),
            Contracts = Contracts.Concat(contract.Contracts)
                .GroupBy(c => c.Id)
                .Select(group =>
                    group.Aggregate((a, b) => a.Merge(b))
                )
        };
    }
}