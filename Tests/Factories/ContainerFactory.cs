using Bogus;
using Docker.DotNet.Models;
using Frierun.Server.Data;

namespace Frierun.Tests.Factories;

public sealed class ContainerFactory : Faker<Container>
{
    public ContainerFactory()
    {
        CustomInstantiator(_ => new Container(""));
        RuleFor(p => p.Name, f => f.Lorem.Word());
        RuleFor(p => p.ContainerName, f => f.Lorem.Word());
        RuleFor(p => p.ImageName, f => f.Internet.Url());
        RuleFor(p => p.MountDockerSocket, f => f.Random.Bool());
        RuleFor(p => p.NetworkName, _ => "");
        RuleFor(p => p.Command, f => new List<string>(f.Lorem.Words()));
        RuleFor(p => p.Env, f => new Dictionary<string, string>());
        RuleFor(p => p.Labels, f => new Dictionary<string, string>());

        RuleSet(
            "udocker", set => { set.RuleFor(p => p.MountDockerSocket, false); }
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