using Docker.DotNet.Models;
using Frierun.Server;
using Frierun.Server.Data;
using NSubstitute;

namespace Frierun.Tests.Installers.Docker;

public class VolumeInstallerTests : BaseTests
{
    public VolumeInstallerTests()
    {
        TryInstallPackage("docker");
    }

    [Fact]
    public void Install_TwoApplicationsWithSameVolume_AddsVolumeOnce()
    {
        var volume = Factory<Volume>().Generate();
        volume = volume with { VolumeName = volume.Name };
        var package1 = Factory<Package>().Generate() with
        {
            Contracts = [volume]
        };
        var package2 = Factory<Package>().Generate() with
        {
            Contracts = [volume]
        };

        var application1 = TryInstallPackage(package1);
        var application2 = TryInstallPackage(package2);

        Assert.NotNull(application1);
        Assert.NotNull(application2);
        var volume1 = application1.Resources.OfType<DockerVolume>().First();
        var volume2 = application2.Resources.OfType<DockerVolume>().First();
        Assert.NotSame(volume1, volume2);

        DockerClient.Volumes.Received(1).CreateAsync(Arg.Any<VolumesCreateParameters>());
    }

    [Fact]
    public void Uninstall_TwoApplicationsWithSameVolume_RemovesVolumeOnce()
    {
        var uninstallService = Resolve<UninstallService>();
        var volume = Factory<Volume>().Generate();
        volume = volume with { VolumeName = volume.Name };
        var package1 = Factory<Package>().Generate() with
        {
            Contracts = [volume]
        };
        var package2 = Factory<Package>().Generate() with
        {
            Contracts = [volume]
        };

        var application1 = TryInstallPackage(package1);
        var application2 = TryInstallPackage(package2);
        Assert.NotNull(application1);
        Assert.NotNull(application2);
        uninstallService.Handle(application1);
        uninstallService.Handle(application2);

        DockerClient.Volumes.Received(1).RemoveAsync(volume.Name, Arg.Any<bool>());
    }
}