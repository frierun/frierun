using Bogus;
using Network = Frierun.Server.Data.Network;

namespace Frierun.Tests.Factories;

public sealed class NetworkFactory: Faker<Network>
{
    private readonly HashSet<string?> _uniqueNames = [];
    
    public NetworkFactory()
    {
        CustomInstantiator(_ => new Network(""));
        this.UniqueRuleFor(p => p.Name, f => f.Lorem.Word(), _uniqueNames);
        RuleFor(p => p.NetworkName, f => f.Lorem.Word());

        RuleSet(
            "udocker", set =>
            {
                RuleFor(p => p.NetworkName, "udocker");
            }
        );
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