using System.Linq.Expressions;
using Bogus;
using Frierun.Server;
using Frierun.Server.Data;
using Frierun.Server.Handlers;

namespace Frierun.Tests.Factories;

public abstract class ContractFaker<TContract> : Faker<TContract>
    where TContract : Contract
{
    public required HandlerRegistry HandlerRegistry { get; init; }
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
    
    /// <summary>
    /// Sets handler for the contract
    /// </summary>
    public ContractFaker<TContract> SetHandler<THandler>(Application? application = null)
        where THandler : Handler<TContract>
    {
        var handler = HandlerRegistry.GetHandler(typeof(THandler).Name, application?.Name);
        RuleFor(p => p.Handler, _ => handler);
        return this;
    }

    /// <summary>
    /// Sets property value
    /// </summary>
    public ContractFaker<TContract> Set<TProperty>(
        Expression<Func<TContract, TProperty>> property,
        TProperty value
    )
    {
        base.RuleFor(property, value);
        return this;
    }
    
    public ContractFaker<TContract> Set<TProperty>(
        Expression<Func<TContract, Argument<TProperty>>> property,
        TProperty value
    )
    {
        base.RuleFor(property, value);
        return this;
    }
    
    /// <summary>
    /// Always insert default ruleset 
    /// </summary>
    protected override string[] ParseDirtyRulesSets(string dirtyRules)
    {
        var result = base.ParseDirtyRulesSets(dirtyRules);
        if (result[0] != "default")
        {
            return
            [
                "default",
                ..result
            ];
        }

        return result;
    }    
}