﻿using Frierun.Server.Data;

namespace Frierun.Tests.Installers.Base;

public class PortHttpEndpointInstallerTests : BaseTests
{
    [Fact]
    public void Install_ContainerWithHttpEndpoint_ContainerDependsOnPortEndpoint()
    {
        var container = Factory<Container>().Generate();
        var package = Factory<Package>().Generate() with
        {
            Contracts =
            [
                container,
                Factory<HttpEndpoint>().Generate() with {ContainerName = container.Name}
            ]
        };

        var application = InstallPackage(package);

        Assert.NotNull(application);
        var dockerContainer = application.DependsOn.OfType<DockerContainer>().First();
        Assert.Contains(dockerContainer.DependsOn, r => r is DockerPortEndpoint);
    }
}