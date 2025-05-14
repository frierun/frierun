using Bogus;
using Frierun.Server.Data;

namespace Frierun.Tests.Factories;

public sealed class ParameterFactory: Faker<Parameter>
{
    private readonly HashSet<string?> _uniqueNames = [];
    
    public ParameterFactory()
    {
        CustomInstantiator(_ => new Parameter(""));
        this.UniqueRuleFor(p => p.Name, f => f.Lorem.Word(), _uniqueNames);
        RuleFor(p => p.DefaultValue, f => f.Lorem.Word());
        RuleFor(p => p.Value, f => f.Lorem.Word());
    }
}