using Bogus;
using Frierun.Server.Data;

namespace Frierun.Tests.Factories;

public sealed class SelectorFactory: ContractFaker<Selector>
{
    public SelectorFactory()
    {
        CustomInstantiator(_ => new Selector(""));
        RuleFor(p => p.Options, _ => []);
    }
}