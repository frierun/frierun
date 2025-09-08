using Bogus;
using Frierun.Server.Data;

namespace Frierun.Tests.Factories;

public sealed class OptionalFactory: ContractFaker<Optional>
{
    public OptionalFactory()
    {
        CustomInstantiator(_ => new Optional(""));
        RuleFor(p => p.Contracts, _ => new ContractList());
    }
}