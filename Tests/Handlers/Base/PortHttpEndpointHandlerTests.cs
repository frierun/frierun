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
        var package = Factory<Package>().Generate() with { Contracts = [container, httpEndpoint] };

        var application = InstallPackage(package);

        var resultHttpEndpoint = application.GetContract(new ContractId<HttpEndpoint>(httpEndpoint.Id));
        Assert.False(resultHttpEndpoint.ResultSsl.Value);
        Assert.Equal(httpEndpoint.Port, resultHttpEndpoint.ResultPort.Value);
        Assert.NotNull(resultHttpEndpoint.ResultHost.Value);
    }

    [Fact]
    public void Install_ContainerWithHttpEndpoint_DependsOnPortEndpoint()
    {
        InstallPackage("docker");
        var container = Factory<Container>().Generate();

        var package = Factory<Package>().Generate() with
        {
            Contracts =
            [
                container,
                Factory<HttpEndpoint>().Generate() with { Container = new ContractId<Container>(container.Id) }
            ]
        };

        var application = InstallPackage(package);

        var httpEndpoint = application.GetContracts<HttpEndpoint>().Single();
        var portEndpoint = application.GetContracts<PortEndpoint>().Single();
        Assert.Contains(portEndpoint.Id, httpEndpoint.DependsOn);
    }
}