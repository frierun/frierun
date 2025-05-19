using Frierun.Server.Data;

namespace Frierun.Tests.Handlers.Docker;

public class PortEndpointHandlerTests : BaseTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Install_ContainerWithPortEndpoint_InstallEndpointFirst(bool reverseOrder)
    {
        InstallPackage("docker");
        var container = Factory<Container>().Generate();
        List<Contract> contracts =
        [
            container,
            Factory<PortEndpoint>().Generate() with { Container = (ContractId<Container>)container.Id }
        ];
        if (reverseOrder)
        {
            contracts.Reverse();
        }        
        var package = Factory<Package>().Generate() with { Contracts = contracts };

        var application = InstallPackage(package);

        var installedContainers = application.Contracts.ToList();
        var endpointIndex = installedContainers.FindIndex(r => r is PortEndpoint);
        var containerIndex = installedContainers.FindIndex(r => r is Container);
        Assert.NotEqual(-1, endpointIndex);
        Assert.NotEqual(-1, containerIndex);
        Assert.True(endpointIndex < containerIndex);        
    }
}