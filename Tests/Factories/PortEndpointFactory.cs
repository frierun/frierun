using Bogus;
using Frierun.Server.Data;

namespace Frierun.Tests.Factories;

public sealed class PortEndpointFactory : Faker<PortEndpoint>
{
    public PortEndpointFactory()
    {
        StrictMode(true);
        CustomInstantiator(f => new PortEndpoint(default, default));
        RuleFor(p => p.Protocol, f => f.Random.Enum<Protocol>());
        RuleFor(p => p.Port, f => f.Internet.Port());
        RuleFor(p => p.ContainerName, f => f.Lorem.Word());
        Ignore(p => p.Name);
        Ignore(p => p.DestinationPort);
        Ignore(p => p.Installer);
        Ignore(p => p.DependsOn);
    }
}