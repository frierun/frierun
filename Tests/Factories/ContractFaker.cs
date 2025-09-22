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
        base.RuleSet(
            "installed", set => { set.RuleFor(p => p.Id, _ => Guid.CreateVersion7()); }
        );
    }
    
    /// <summary>
    /// Generates contract entry
    /// </summary>
    public new ContractEntry<TContract> Generate(string? ruleSets = null)
    {
        var result = base.Generate(ruleSets);
        
        int tries = 0;
        string name;
        do
        {
            if (++tries > 100)
            {
                throw new InvalidOperationException("Could not generate a unique value");
            }
            name = FakerHub.Lorem.Word();
        } while (!_uniqueNames.Add(name));
        
        var contractRef = new ContractRef<TContract>(name);
        return new ContractEntry<TContract>(contractRef, result);
    }

    /// <summary>
    /// Generates a list of contract entries
    /// </summary>
    public new List<ContractEntry<TContract>> Generate(int count, string? ruleSets = null)
    {
        return Enumerable.Range(1, count)
            .Select(_ => this.Generate(ruleSets))
            .ToList();
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