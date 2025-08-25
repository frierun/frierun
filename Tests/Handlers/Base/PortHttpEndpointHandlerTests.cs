using Frierun.Server.Data;

namespace Frierun.Tests.Handlers.Base;

public class PortHttpEndpointHandlerTests : BaseTests
{
    [Fact]
    public void Install_ContainerWithHttpEndpoint_CreatesEndpoint()
    {
        InstallPackage("docker");
        var container = Factory<Container>().Generate();
        var httpEndpoint = Factory<HttpEndpoint>().Generate() with
        {
            Container = new ContractId<Container>(container.Name)
        };
        List<Contract> contracts =
        [
            container,
            httpEndpoint
        ];
        var package = Factory<Package>().Generate() with { Contracts = contracts };

        var application = InstallPackage(package);

        var resultHttpEndpoint = application.Contracts.OfType<HttpEndpoint>().Single();
        Assert.False(resultHttpEndpoint.ResultSsl.Value);
        Assert.Equal(httpEndpoint.Port, resultHttpEndpoint.ResultPort.Value);
        Assert.NotNull(resultHttpEndpoint.ResultHost.Value);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Install_ContainerWithHttpEndpoint_InstallContainerFirst(bool reverseOrder)
    {
        InstallPackage("docker");
        var container = Factory<Container>().Generate();
        List<Contract> contracts =
        [
            container,
            Factory<HttpEndpoint>().Generate() with { Container = new ContractId<Container>(container.Name) }
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
        Assert.True(containerIndex < endpointIndex);
    }
}