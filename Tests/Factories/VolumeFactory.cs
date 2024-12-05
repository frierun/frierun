using Bogus;
using Frierun.Server.Data;
using Frierun.Server.Services;

namespace Frierun.Tests.Factories;

public sealed class VolumeFactory : Faker<DockerVolume>
{
    private readonly HashSet<string> _uniqueNames = [];
    
    public VolumeFactory(PackageRegistry packageRegistry)
    {
        StrictMode(true);
        this.SkipConstructor();
        RuleFor(p => p.Name, f => f.Lorem.Word());
    }
}