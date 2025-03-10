﻿using Frierun.Server.Data;
using Mount = Frierun.Server.Data.Mount;

namespace Frierun.Tests.Installers.Docker;

public class MountInstallerTests : BaseTests
{
    [Fact]
    public void Install_ContainerWithMount_CreatesVolume()
    {
        var container = Factory<Container>().Generate();
        var package = Factory<Package>().Generate() with
        {
            Contracts = [
                container,
                Factory<Mount>().Generate() with { ContainerName = container.Name }
            ]
        };
        
        var application = InstallPackage(package);

        Assert.NotNull(application);
        var dockerContainer = application.DependsOn.OfType<DockerContainer>().First();
        Assert.Contains(dockerContainer.DependsOn, r => r is DockerVolume);
    }
}