using Docker.DotNet;
using Docker.DotNet.Models;
using Frierun.Server;
using Frierun.Server.Data;
using NSubstitute;

namespace Frierun.Tests.Installers.Docker;

public class VolumeInstallerTests : BaseTests
{
    [Fact]
    public void Install_TwoApplicationsWithSameVolume_AddsVolumeOnce()
    {
        var dockerClient = Resolve<IDockerClient>();
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

        var application1 = InstallPackage(package1);
        var application2 = InstallPackage(package2);

        Assert.NotNull(application1);
        Assert.NotNull(application2);
        var volume1 = application1.DependsOn.OfType<DockerVolume>().First();
        var volume2 = application2.DependsOn.OfType<DockerVolume>().First();
        Assert.NotEqual(volume1, volume2);

        dockerClient.Volumes.Received(1).CreateAsync(Arg.Any<VolumesCreateParameters>());
    }

    [Fact]
    public void Uninstall_TwoApplicationsWithSameVolume_RemovesVolumeOnce()
    {
        var dockerClient = Resolve<IDockerClient>();
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

        var application1 = InstallPackage(package1);
        var application2 = InstallPackage(package2);
        Assert.NotNull(application1);
        Assert.NotNull(application2);
        uninstallService.Handle(application1);
        uninstallService.Handle(application2);

        dockerClient.Volumes.Received(1).RemoveAsync(volume.Name, Arg.Any<bool>());
    }
}