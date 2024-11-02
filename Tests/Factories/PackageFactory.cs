using Bogus;
using Frierun.Server.Models;
using Frierun.Server.Resources;
using Frierun.Server.Services;

namespace Frierun.Tests.Factories;

public sealed class PackageFactory : Faker<Package>
{
    private readonly HashSet<string?> _uniqueNames = [];
    
    public PackageFactory(PackageRegistry packageRegistry, Faker<ContainerDefinition> containerDefinitionFactory)
    {
        StrictMode(true);
        this.SkipConstructor();
        this.UniqueRuleFor(p => p.Name, f => f.Lorem.Word(), _uniqueNames);
        RuleFor(p => p.Url, f => f.Internet.Url());
        RuleFor(p => p.Children, _ => new List<ResourceDefinition>(containerDefinitionFactory.Generate(1)));
        FinishWith((_, package) => packageRegistry.Packages.Add(package));
    }
}