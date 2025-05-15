using Frierun.Server.Data;

namespace Frierun.Tests.Installers.Base;

public class PortHttpEndpointInstallerTests : BaseTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Install_ContainerWithHttpEndpoint_InstallEndpointFirst(bool reverseOrder)
    {
        InstallPackage("docker");
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

        var installedContracts = application.Contracts.ToList();
        var endpointIndex = installedContracts.FindIndex(r => r is PortEndpoint);
        var containerIndex = installedContracts.FindIndex(r => r is Container);
        Assert.NotEqual(-1, endpointIndex);
        Assert.NotEqual(-1, containerIndex);
        Assert.True(endpointIndex < containerIndex);
    }
}