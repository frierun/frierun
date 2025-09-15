using Frierun.Server.Data;

namespace Frierun.Tests.Factories;

public sealed class HttpEndpointFactory : ContractFaker<HttpEndpoint>
{
    public HttpEndpointFactory()
    {
        CustomInstantiator(f => new HttpEndpoint());
        RuleFor(p => p.Port, f => f.Internet.Port());
        RuleFor(p => p.Container, f => new ContractId<Container>(f.Lorem.Word()));
    }
}