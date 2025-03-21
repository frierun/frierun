using Frierun.Server.Data;

namespace Frierun.Tests.Installers.Base;

public class PortHttpEndpointInstallerTests : BaseTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Install_ContainerWithHttpEndpoint_InstallEndpointFirst(bool reverseOrder)
    {
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
        var endpointIndex = resources.FindIndex(r => r is DockerPortEndpoint);
        var containerIndex = resources.FindIndex(r => r is DockerContainer);
        Assert.True(endpointIndex < containerIndex);
    }
}