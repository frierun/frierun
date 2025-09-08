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
        var (containerId, container) = Contract<Container>().GenerateEntry();
        var (portId, port) = Contract<PortEndpoint>().GenerateEntry();
        var package = Factory<Package>().Generate() with
        {
            Contracts = new ContractList
            {
                [containerId] = container,
                [portId] = port with { Container = containerId }
            }
        };

        var application = InstallPackage(package);

        Assert.Contains(containerId, application.GetContracts<PortEndpoint>().Single().DependsOn);
    }

    [Fact]
    public void Install_ContainerWithPortEndpoint_PassesPortToContainer()
    {
        InstallPackage("docker");
        var (containerId, container) = Contract<Container>().GenerateEntry();
        var (portId, port) = Contract<PortEndpoint>().GenerateEntry();
        var package = Factory<Package>().Generate() with
        {
            Contracts = new ContractList
            {
                [containerId] = container,
                [portId] = port with { Container = containerId }
            }
        };

        var application = InstallPackage(package);

        var installedPort = application.GetContract(portId);
        Assert.True(installedPort.Installed);

        DockerClient.Containers.Received(1).CreateContainerAsync(
            Arg.Is<CreateContainerParameters>(p =>
                p.HostConfig
                    .PortBindings[$"{installedPort.Port}/{installedPort.Protocol.ToString().ToLower()}"][0]
                    .HostPort == installedPort.Port.ToString()
            ),
            Arg.Any<CancellationToken>()
        );
    }
}