using Bogus;
using Frierun.Server.Data;

namespace Frierun.Tests.Factories;

public sealed class PortEndpointFactory : Faker<PortEndpoint>
{
    public PortEndpointFactory(Faker<Container> containerFactory)
    {
        CustomInstantiator(f => new PortEndpoint(default, 0));
        RuleFor(p => p.Protocol, f => f.Random.Enum<Protocol>());
        RuleFor(p => p.Port, f => f.Internet.Port());
        RuleFor(p => p.Container, _ => containerFactory.Generate().Id);
    }
}