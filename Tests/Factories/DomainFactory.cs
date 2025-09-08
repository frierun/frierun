using Bogus;
using Frierun.Server.Data;

namespace Frierun.Tests.Factories;

public sealed class DomainFactory: ContractFaker<Domain>
{
    public DomainFactory()
    {
        CustomInstantiator(_ => new Domain(""));
    }
}