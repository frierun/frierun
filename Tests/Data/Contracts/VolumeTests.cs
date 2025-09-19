using Frierun.Server.Data;
using Frierun.Server.Handlers.Docker;

namespace Frierun.Tests.Data.Contracts;

public class VolumeTests : BaseTests
{
    [Fact]
    public void Merge_ContractsWithDifferentDependencies_KeepsDependencies()
    {
        var dependency = new ContractRef<Container>("test");
        var dependency2 = new ContractRef<Container>("test2");
        var volume = Factory<Volume>().Generate() with { DependsOn = [dependency] };

        var result = volume.Merge(volume with { DependsOn = [dependency2] });

        Assert.Equal([dependency, dependency2], result.DependsOn);
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