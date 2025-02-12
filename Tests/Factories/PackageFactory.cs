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
        CustomInstantiator(_ => new Package(""));
        this.UniqueRuleFor(p => p.Name, f => f.Lorem.Word(), _uniqueNames);
        RuleFor(p => p.Prefix, (_, p) => p.Name);
        RuleFor(p => p.Url, f => f.Internet.Url());
        RuleFor(p => p.ApplicationUrl, f => f.Internet.Url());
        RuleFor(p => p.ApplicationDescription, f => f.Lorem.Sentence());
        RuleFor(p => p.ShortDescription, f => f.Lorem.Sentence());
        RuleFor(p => p.FullDescription, f => f.Lorem.Paragraph());
        RuleFor(p => p.Tags, f => new List<string>(f.Lorem.Words()));
        RuleFor(p => p.Installer, _ => null);
        RuleFor(p => p.DependsOn, _ => Array.Empty<Resource>());
        RuleFor(p => p.Contracts, _ => Array.Empty<Contract>());
        FinishWith((_, package) => packageRegistry.Packages.Add(package));
    }
}