using Docker.DotNet.Models;
using Frierun.Server.Data;
using NSubstitute;

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

    [Fact]
    public void Install_ContainerWithPortEndpoint_PassesPortToContainer()
    {
        InstallPackage("docker");
        var container = Factory<Container>().Generate();
        List<Contract> contracts =
        [
            container,
            Factory<PortEndpoint>().Generate() with { Container = (ContractId<Container>)container.Id }
        ];
        var package = Factory<Package>().Generate() with { Contracts = contracts };

        var application = InstallPackage(package);

        var endpoint = application.Contracts.OfType<PortEndpoint>().Single();
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