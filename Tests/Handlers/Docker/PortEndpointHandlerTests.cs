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
        var container = Contract<Container>().Generate();
        var portEndpoint = Contract<PortEndpoint>().Set(p => p.Container, container.Ref).Generate();
        var package = Factory<Package>().Generate() with
        {
            Contracts = [container, portEndpoint]
        };

        var application = InstallPackage(package);


        Assert.Contains(
            State.GetContract(application, container.Ref).Id, 
            State.GetContract(application, portEndpoint.Ref).DependsOn
        );
    }

    [Fact]
    public void Install_ContainerWithPortEndpoint_PassesPortToContainer()
    {
        InstallPackage("docker");
        var container = Contract<Container>().Generate();
        var port = Contract<PortEndpoint>().Set(p => p.Container, container.Ref).Generate();
        var package = Factory<Package>().Generate() with
        {
            Contracts = [container, port]
        };

        var application = InstallPackage(package);

        var installedPort = State.GetContract(application, port.Ref);
        ;
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