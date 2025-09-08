using Bogus;
using Frierun.Server.Data;

namespace Frierun.Tests.Factories;

public abstract class ContractFaker<TContract> : Faker<TContract>
    where TContract : Contract
{
    private readonly HashSet<string?> _uniqueNames = [];

    protected ContractFaker()
    {
        this.UniqueRuleFor(p => p.Name, f => f.Lorem.Word(), _uniqueNames);
    }

    /// <summary>
    /// Generates name and contract
    /// </summary>
    public new ContractEntry<TContract> Generate(string? ruleSets = null)
    {
        var result = base.Generate(ruleSets);
        var contractId = new ContractId<TContract>(result.Name);
        return new ContractEntry<TContract>(contractId, result);
    }

    public ContractEntry<TContract> GenerateEntry()
    {
        return Generate();
    }

}