using Frierun.Server.Data;

namespace Frierun.Tests.Factories;

public sealed class ParameterFactory: ContractFaker<Parameter>
{
    public ParameterFactory()
    {
        CustomInstantiator(_ => new Parameter());
        RuleFor(p => p.DefaultValue, f => f.Lorem.Word());
        RuleFor(p => p.Value, f => new Argument<string>(f.Lorem.Word()));
    }
}