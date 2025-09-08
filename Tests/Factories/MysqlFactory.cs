using Bogus;
using Frierun.Server.Data;

namespace Frierun.Tests.Factories;

public sealed class MysqlFactory: ContractFaker<Mysql>
{
    public MysqlFactory()
    {
        CustomInstantiator(_ => new Mysql(""));
    }
}