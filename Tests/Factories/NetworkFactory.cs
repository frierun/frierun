using Bogus;
using Network = Frierun.Server.Data.Network;

namespace Frierun.Tests.Factories;

public sealed class NetworkFactory: Faker<Network>
{
    public NetworkFactory()
    {
        CustomInstantiator(_ => new Network(""));
        RuleFor(p => p.Name, f => f.Lorem.Word());
        RuleFor(p => p.NetworkName, f => f.Lorem.Word());
    }
}