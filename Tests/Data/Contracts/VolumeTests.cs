using Frierun.Server.Data;

namespace Frierun.Tests.Data.Contracts;

public class VolumeTests : BaseTests
{
    [Fact]
    public void With_KeepsDependencies()
    {
        var dependency = new ContractId<Container>("test");
        var dependency2 = new ContractId<Container>("test2");
        var volume = Factory<Volume>().Generate()
            with
            {
                DependsOn = [dependency],
                DependencyOf = [dependency2],
            };
        
        var result = volume.With(Factory<Volume>().Generate() with { Name = volume.Name});

        Assert.Equal([dependency], result.DependsOn);
        Assert.Equal([dependency2], result.DependencyOf);
    }
}