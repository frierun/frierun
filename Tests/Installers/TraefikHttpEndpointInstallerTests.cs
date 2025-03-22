using Frierun.Server;
using Frierun.Server.Data;
using Frierun.Server.Services;

namespace Frierun.Tests.Installers;

public class TraefikHttpEndpointInstallerTests : BaseTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Install_ContainerWithHttpEndpoint_InstallTraefikFirst(bool reverseOrder)
    {
        var providerApplication = InstallPackage("traefik");
        Assert.NotNull(providerApplication);

        var container = Factory<Container>().Generate();
        List<Contract> contracts =
        [
            container,
            Factory<HttpEndpoint>().Generate() with { ContainerName = container.Name }
        ];
        if (reverseOrder)
        {
            contracts.Reverse();
        }
        var package = Factory<Package>().Generate() with { Contracts = contracts };

        var application = InstallPackage(package);

        Assert.NotNull(application);
        var resources = application.Resources.ToList();
        var endpointIndex = resources.FindIndex(r => r is TraefikHttpEndpoint);
        var containerIndex = resources.FindIndex(r => r is DockerContainer);
        Assert.True(endpointIndex < containerIndex);
    }
}