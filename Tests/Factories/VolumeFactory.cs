using Bogus;
using Frierun.Server.Resources;
using Frierun.Server.Services;

namespace Frierun.Tests.Factories;

public sealed class VolumeFactory : Faker<Volume>
{
    private readonly HashSet<string> _uniqueNames = [];
    
    public VolumeFactory(PackageRegistry packageRegistry)
    {
        StrictMode(true);
        this.SkipConstructor();
        RuleFor(p => p.Name, f => f.Lorem.Word());
    }
}