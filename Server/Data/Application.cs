using static Frierun.Server.Data.Merger;

namespace Frierun.Server.Data;

public record Application(
    string Name,
    Package? Package = null,
    Argument<string>? Url = null,
    Argument<string>? Description = null,
    IReadOnlyList<string>? RequiredApplications = null,
    IReadOnlyDictionary<ContractRef, Guid>? ContractRefs = null
) : Contract
{
    public Argument<string> Url { get; init; } = Url ?? new Argument<string>();
    public Argument<string> Description { get; init; } = Description ?? new Argument<string>();
    public IReadOnlyList<string> RequiredApplications { get; init; } = RequiredApplications ?? [];

    public IReadOnlyDictionary<ContractRef, Guid> ContractRefs { get; init; } =
        ContractRefs ?? new Dictionary<ContractRef, Guid>();

    public override IEnumerable<IArgument> GetArguments()
    {
        yield return Url;
        yield return Description;
        foreach (var argument in base.GetArguments())
        {
            yield return argument;
        }
    }

    public override Application Merge(Contract other)
    {
        return MergeCommon(this, other, out var contract) with
        {
            Name = MergeValue(Name, contract.Name),
            Url = Url.Merge(contract.Url),
            Description = Description.Merge(contract.Description),
        };
    }
}