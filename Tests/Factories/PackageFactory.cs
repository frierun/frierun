using Bogus;
using Frierun.Server.Models;
using Frierun.Server.Services;

namespace Frierun.Tests.Factories;

public sealed class PackageFactory : Faker<Package>
{
    private readonly HashSet<string> _uniqueNames = [];
    
    public PackageFactory(PackageRegistry packageRegistry, Faker<Volume> volumeFactory)
    {
        StrictMode(true);
        this.SkipConstructor();
        this.UniqueRuleFor(p => p.Name, f => f.Lorem.Word(), _uniqueNames);
        RuleFor(p => p.ImageName, f => f.Internet.Url());
        RuleFor(p => p.Port, f => f.Random.Number(1, 65535));
        RuleFor(p => p.RequireDocker, f => f.Random.Bool());
        RuleFor(p => p.Volumes, f => volumeFactory.Generate(f.Random.Number(0, 5)).ToList());
        FinishWith((_, package) => packageRegistry.Packages.Add(package));
    }
}