using Bogus;
using Frierun.Server.Data;

namespace Frierun.Tests.Factories;

public sealed class VolumeFactory: Faker<Volume>
{
    private readonly HashSet<string?> _uniqueNames = [];
    
    public VolumeFactory()
    {
        CustomInstantiator(_ => new Volume(""));
        this.UniqueRuleFor(p => p.Name, f => f.Lorem.Word(), _uniqueNames);
    }
}