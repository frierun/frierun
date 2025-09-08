using Frierun.Server.Data;

namespace Frierun.Tests.Handlers.Base;

public class PortHttpEndpointHandlerTests : BaseTests
{
    [Fact]
    public void Install_ContainerWithHttpEndpoint_CreatesEndpoint()
    {
        InstallPackage("docker");
        var (containerId, container) = Contract<Container>().GenerateEntry();
        var (httpEndpointId, httpEndpoint) = Contract<HttpEndpoint>().GenerateEntry();
        httpEndpoint = httpEndpoint with { Container = containerId };
        var package = Factory<Package>().Generate() with
        {
            Contracts = new ContractList { [containerId] = container, [httpEndpointId] = httpEndpoint }
        };

        var application = InstallPackage(package);

        var resultHttpEndpoint = application.GetContract(httpEndpointId);
        Assert.False(resultHttpEndpoint.ResultSsl.Value);
        Assert.Equal(httpEndpoint.Port, resultHttpEndpoint.ResultPort.Value);
        Assert.NotNull(resultHttpEndpoint.ResultHost.Value);
    }

    [Fact]
    public void Install_ContainerWithHttpEndpoint_DependsOnPortEndpoint()
    {
        InstallPackage("docker");
        var (containerId, container) = Contract<Container>().GenerateEntry();
        var (httpEndpointId, httpEndpoint) = Contract<HttpEndpoint>().GenerateEntry();
        httpEndpoint = httpEndpoint with { Container = containerId };

        var package = Factory<Package>().Generate() with
        {
            Contracts = new ContractList { [containerId] = container, [httpEndpointId] = httpEndpoint }
        };

        var application = InstallPackage(package);

        Assert.Contains(
            application.GetContracts<PortEndpoint>().Single().Id,
            application.GetContracts<HttpEndpoint>().Single().DependsOn
        );
    }
}