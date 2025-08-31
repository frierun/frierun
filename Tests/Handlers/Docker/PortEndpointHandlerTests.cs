using Docker.DotNet.Models;
using Frierun.Server.Data;
using NSubstitute;

namespace Frierun.Tests.Handlers.Docker;

public class PortEndpointHandlerTests : BaseTests
{
    [Fact]
    public void Install_ContainerWithPortEndpoint_DependsOnContainer()
    {
        InstallPackage("docker");
        var container = Factory<Container>().Generate();
        var package = Factory<Package>().Generate() with
        {
            Contracts =
            [
                container,
                Factory<PortEndpoint>().Generate() with { Container = new ContractId<Container>(container.Id) }
            ]
        };

        var application = InstallPackage(package);

        var portEndpoint = application.GetContracts<PortEndpoint>().Single();
        Assert.Contains(container.Id, portEndpoint.DependsOn);
    }

    [Fact]
    public void Install_ContainerWithPortEndpoint_PassesPortToContainer()
    {
        InstallPackage("docker");
        var container = Factory<Container>().Generate();
        var package = Factory<Package>().Generate() with
        {
            Contracts =
            [
                container,
                Factory<PortEndpoint>().Generate() with { Container = new ContractId<Container>(container.Id) }
            ]
        };

        var application = InstallPackage(package);

        var endpoint = application.GetContracts<PortEndpoint>().Single();
        Assert.True(endpoint.Installed);

        DockerClient.Containers.Received(1).CreateContainerAsync(
            Arg.Is<CreateContainerParameters>(p =>
                p.HostConfig
                    .PortBindings[$"{endpoint.Port}/{endpoint.Protocol.ToString().ToLower()}"][0]
                    .HostPort == endpoint.Port.ToString()
            ),
            Arg.Any<CancellationToken>()
        );
    }
}