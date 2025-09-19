using System.Collections.ObjectModel;
using static Frierun.Server.Data.Merger;

namespace Frierun.Server.Data;

public record Application(
    string Name,
    Package? Package = null,
    Argument<string>? Url = null,
    Argument<string>? Description = null,
    ContractList? Contracts = null,
    IReadOnlyList<string>? RequiredApplications = null,
    IReadOnlyDictionary<ContractId, Guid>? ContractRefs = null
) : Contract
{
    public Argument<string> Url { get; init; } = Url ?? new Argument<string>();
    public Argument<string> Description { get; init; } = Description ?? new Argument<string>();
    public IReadOnlyList<string> RequiredApplications { get; init; } = RequiredApplications ?? [];
    public ContractList Contracts { get; init; } = Contracts ?? new ContractList();
    public IReadOnlyDictionary<ContractId, Guid> ContractRefs { get; init; } = ContractRefs ?? new Dictionary<ContractId, Guid>();

    public override IEnumerable<IArgument> GetArguments()
    {
        yield return Url;
        yield return Description;
    }

    /// <summary>
    /// Get contract by id.
    /// </summary>
    public T GetContract<T>(ContractId<T> contractId)
        where T : Contract
    {
        return (T)Contracts[contractId];
    }

    /// <summary>
    /// Gets all contracts of the specified type
    /// </summary>
    public IEnumerable<T> GetContracts<T>()
        where T : Contract
    {
        return Contracts.Values.OfType<T>();
    }

    public override Application Merge(Contract other)
    {
        var contract = EnsureSame(this, other);

        return MergeCommon(this, contract) with
        {
            Name = OnlyOne(Name, contract.Name),
            Url = Url.Merge(contract.Url),
            Description = Description.Merge(contract.Description),
            Contracts = Contracts.Merge(contract.Contracts)
        };
    }
}