﻿using Docker.DotNet.Models;
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

        var volume1 = application1.Contracts.OfType<Volume>().Single();
        var volume2 = application2.Contracts.OfType<Volume>().Single();
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

        var application1 = InstallPackage(package1);
        var application2 = InstallPackage(package2);
        uninstallService.Handle(application1);
        uninstallService.Handle(application2);

        DockerClient.Volumes.Received(1).RemoveAsync(volume.Name, Arg.Any<bool>());
    }
}