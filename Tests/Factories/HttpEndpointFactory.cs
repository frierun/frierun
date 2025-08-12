using Bogus;
using Frierun.Server.Data;

namespace Frierun.Tests.Factories;

public sealed class HttpEndpointFactory : Faker<HttpEndpoint>
{
    private readonly HashSet<string?> _uniqueNames = [];
    
    public HttpEndpointFactory()
    {
        CustomInstantiator(f => new HttpEndpoint());
        this.UniqueRuleFor(p => p.Name, f => f.Lorem.Word(), _uniqueNames);
        RuleFor(p => p.Port, f => f.Internet.Port());
        RuleFor(p => p.Container, f => new ContractId<Container>(f.Lorem.Word()));
    }
}