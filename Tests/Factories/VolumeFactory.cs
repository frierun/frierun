using Bogus;
using Frierun.Server.Data;

namespace Frierun.Tests.Factories;

public sealed class VolumeFactory: Faker<Volume>
{
    private readonly HashSet<string?> _uniqueNames = [];
    
    public VolumeFactory()
    {
        StrictMode(true);
        CustomInstantiator(_ => new Volume(""));
        this.UniqueRuleFor(p => p.Name, f => f.Lorem.Word(), _uniqueNames);
        Ignore(p => p.Path);
        Ignore(p => p.VolumeName);
        Ignore(p => p.Installer);
        Ignore(p => p.DependsOn);
        Ignore(p => p.DependencyOf);
    }
}