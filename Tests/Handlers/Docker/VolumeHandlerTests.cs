using Bogus;
using Docker.DotNet.Models;
using Frierun.Server;
using Frierun.Server.Data;
using NSubstitute;

namespace Frierun.Tests.Handlers.Docker;

public class VolumeHandlerTests : BaseTests
{
    public VolumeHandlerTests()
    {
        InstallPackage("docker");
    }

    [Fact]
    public void Install_TwoApplicationsWithSameVolume_AddsVolumeOnce()
    {
        var volumeName = Resolve<Faker>().Lorem.Word();
        var volume = Contract<Volume>().Set(v => v.VolumeName, volumeName).Generate();
        var package1 = Factory<Package>().Generate() with { Contracts = [volume] };
        var package2 = Factory<Package>().Generate() with { Contracts = [volume] };

        var application1 = InstallPackage(package1);
        var application2 = InstallPackage(package2);

        var volume1 = State.GetContract(application1, volume.Ref);
        var volume2 = State.GetContract(application2, volume.Ref);
        Assert.NotSame(volume1, volume2);

        DockerClient.Volumes.Received(1).CreateAsync(Arg.Is<VolumesCreateParameters>(arg => arg.Name == volumeName));
    }

    [Fact]
    public void Uninstall_TwoApplicationsWithSameVolume_RemovesVolumeOnce()
    {
        var uninstallService = Resolve<UninstallService>();
        var volumeName = Resolve<Faker>().Lorem.Word();
        var volume = Contract<Volume>().Set(v => v.VolumeName, volumeName).Generate();
        var package1 = Factory<Package>().Generate() with { Contracts = [volume] };
        var package2 = Factory<Package>().Generate() with { Contracts = [volume] };

        var application1 = InstallPackage(package1);
        var application2 = InstallPackage(package2);
        uninstallService.Handle(application1);
        uninstallService.Handle(application2);

        DockerClient.Volumes.Received(1).RemoveAsync(volumeName, Arg.Any<bool>());
    }
}