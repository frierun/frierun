using Bogus;
using Frierun.Server.Data;
using Frierun.Server.Services;

namespace Frierun.Tests.Factories;

public sealed class PackageFactory : Faker<Package>
{
    private readonly HashSet<string?> _uniqueNames = [];
    
    public PackageFactory(PackageRegistry packageRegistry, Faker<Container> containerDefinitionFactory)
    {
        StrictMode(true);
        this.SkipConstructor();
        this.UniqueRuleFor(p => p.Name, f => f.Lorem.Word(), _uniqueNames);
        RuleFor(p => p.Url, f => f.Internet.Url());
        RuleFor(p => p.Contracts, _ => new List<Contract>(containerDefinitionFactory.Generate(1)));
        FinishWith((_, package) => packageRegistry.Packages.Add(package));
    }
}