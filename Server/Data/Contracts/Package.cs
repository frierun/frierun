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
    IEnumerable<Contract>? Contracts = null,
    Application? Result = null
) : Contract(Name)
{
    public IReadOnlyList<string> Tags { get; init; } = Tags ?? [];
    public IEnumerable<Contract> Contracts { get; init; } = Contracts ?? [];
    public Argument<string> ApplicationUrl { get; init; } = ApplicationUrl ?? new Argument<string>();
    public Argument<string> ApplicationDescription { get; init; } = ApplicationDescription ?? new Argument<string>();

    public override IEnumerable<IArgument> GetArguments()
    {
        yield return ApplicationUrl;
        yield return ApplicationDescription;
    }

    public override Contract Merge(Contract other)
    {
        var contract = EnsureSame(this, other);

        return MergeCommon(this, contract) with
        {
            Prefix = OnlyOne(Prefix, contract.Prefix),
            ApplicationUrl = ApplicationUrl.Merge(contract.ApplicationUrl),
            ApplicationDescription = ApplicationDescription.Merge(contract.ApplicationDescription),
            Contracts = Contracts.Concat(contract.Contracts)
                .GroupBy(c => c.Id)
                .Select(group =>
                    group.Aggregate((a, b) => a.Merge(b))
                )
        };
    }
}