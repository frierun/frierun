using Frierun.Server.Data;

namespace Frierun.Tests.Factories;

public sealed class PasswordFactory: ContractFaker<Password>
{
    public PasswordFactory()
    {
        CustomInstantiator(_ => new Password());
    }
}