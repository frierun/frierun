using Bogus;
using Frierun.Server.Data;

namespace Frierun.Tests.Factories;

public sealed class HttpEndpointFactory : Faker<HttpEndpoint>
{
    public HttpEndpointFactory()
    {
        StrictMode(true);
        CustomInstantiator(f => new HttpEndpoint());
        RuleFor(p => p.Name, f => f.Lorem.Word());
        RuleFor(p => p.Port, f => f.Internet.Port());
        RuleFor(p => p.ContainerName, f => f.Lorem.Word());
        Ignore(p => p.DomainName);
        Ignore(p => p.Installer);
        Ignore(p => p.DependsOn);
    }
}