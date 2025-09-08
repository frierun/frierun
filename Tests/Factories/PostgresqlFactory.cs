using Bogus;
using Frierun.Server.Data;

namespace Frierun.Tests.Factories;

public sealed class PostgresqlFactory: ContractFaker<Postgresql>
{
    public PostgresqlFactory()
    {
        CustomInstantiator(_ => new Postgresql(""));
    }
}