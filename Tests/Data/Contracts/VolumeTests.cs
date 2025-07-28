using Frierun.Server.Data;
using Frierun.Server.Handlers.Docker;

namespace Frierun.Tests.Data.Contracts;

public class VolumeTests : BaseTests
{
    [Fact]
    public void Merge_ContractsWithDifferentDependencies_KeepsDependencies()
    {
        var dependency = new ContractId<Container>("test");
        var dependency2 = new ContractId<Container>("test2");
        var volume = Factory<Volume>().Generate()
            with
            {
                DependsOn = [dependency],
                DependencyOf = [dependency2],
            };

        var result = volume.Merge(Factory<Volume>().Generate() with { Name = volume.Name });

        Assert.Equal([dependency], result.DependsOn);
        Assert.Equal([dependency2], result.DependencyOf);
    }


    [Fact]
    public void Merge_ContractsWithDifferentHandlers_ThrowsException()
    {
        var docker1 = InstallPackage("docker");
        var docker2 = InstallPackage("docker");
        var volume1 = Factory<Volume>().Generate() with { Handler = Handler<NewVolumeHandler>(docker1) };
        var volume2 = Factory<Volume>().Generate() with { Handler = Handler<NewVolumeHandler>(docker2) };

        Assert.Throws<MergeException>(() => volume1.Merge(volume2));
    }

    [Fact]
    public void Merge_ContractsWithConflictingHandlerApplication_ThrowsException()
    {
        var docker1 = InstallPackage("docker");
        var docker2 = InstallPackage("docker");
        var volume1 = Factory<Volume>().Generate() with { Handler = Handler<NewVolumeHandler>(docker1) };
        var volume2 = Factory<Volume>().Generate() with { HandlerApplication = docker2.Name };

        Assert.Throws<MergeException>(() => volume1.Merge(volume2));
    }
}