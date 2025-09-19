using Frierun.Server.Data;

namespace Frierun.Tests.Handlers.Base;

public class PortHttpEndpointHandlerTests : BaseTests
{
    [Fact]
    public void Install_ContainerWithHttpEndpoint_CreatesEndpoint()
    {
        InstallPackage("docker");
        var container = Contract<Container>().Generate();
        var httpEndpoint = Contract<HttpEndpoint>().Set(p => p.Container, container.Ref).Generate();
        var package = Factory<Package>().Generate() with
        {
            Contracts = [container, httpEndpoint]
        };

        var application = InstallPackage(package);

        var resultHttpEndpoint = State.GetContract(application, httpEndpoint.Ref);
        Assert.False(resultHttpEndpoint.ResultSsl.Value);
        Assert.Equal(httpEndpoint.Contract.Port, resultHttpEndpoint.ResultPort.Value);
        Assert.NotNull(resultHttpEndpoint.ResultHost.Value);
    }

    [Fact]
    public void Install_ContainerWithHttpEndpoint_DependsOnPortEndpoint()
    {
        InstallPackage("docker");
        var container = Contract<Container>().Generate();
        var httpEndpoint = Contract<HttpEndpoint>().Set(p => p.Container, container.Ref).Generate();
        var package = Factory<Package>().Generate() with
        {
            Contracts = [container, httpEndpoint]
        };

        var application = InstallPackage(package);

        Assert.Contains(
            new ContractRef<PortEndpoint>(httpEndpoint.Ref.Name),
            State.GetContract(application, httpEndpoint.Ref).DependsOn
        );
    }
}