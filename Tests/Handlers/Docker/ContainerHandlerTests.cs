using Docker.DotNet.Models;
using Frierun.Server.Data;
using Frierun.Server.Handlers.Docker;
using NSubstitute;
using Network = Frierun.Server.Data.Network;

namespace Frierun.Tests.Handlers.Docker;

public class ContainerHandlerTests : BaseTests
{
    [Fact]
    public void Install_Container_CreatesNetwork()
    {
        InstallPackage("docker");
        var package = Factory<Package>().Generate() with
        {
            Contracts = [Factory<Container>().Generate()]
        };

        var application = InstallPackage(package);

        Assert.True(application.Contracts.OfType<Network>().Single().Installed);
    }

    [Fact]
    public void Install_RequireDocker_MountsSocket()
    {
        InstallPackage("docker");
        var package = Factory<Package>().Generate() with
        {
            Contracts = [Factory<Container>().Generate() with { MountDockerSocket = true }]
        };

        InstallPackage(package);

        DockerClient.Containers.Received(1)
            .CreateContainerAsync(
                Arg.Is<CreateContainerParameters>(p =>
                    p.HostConfig.Mounts.Count == 1 &&
                    p.HostConfig.Mounts[0].Source == "/var/run/docker.sock" &&
                    p.HostConfig.Mounts[0].Target == "/var/run/docker.sock"
                )
            );
    }

    [Fact]
    public void Install_RequireDockerWithPodman_MountsSocket()
    {
        var path = "/run/podman/podman.sock";
        Handler<FakeDockerApiConnectionHandler>().SocketRootPath = path;
        InstallPackage("docker");
        var package = Factory<Package>().Generate() with
        {
            Contracts = [Factory<Container>().Generate() with { MountDockerSocket = true }]
        };

        InstallPackage(package);

        DockerClient.Containers.Received(1)
            .CreateContainerAsync(
                Arg.Is<CreateContainerParameters>(p =>
                    p.HostConfig.Mounts.Count == 1 &&
                    p.HostConfig.Mounts[0].Source == path &&
                    p.HostConfig.Mounts[0].Target == "/var/run/docker.sock"
                )
            );
    }

    [Fact]
    public void Install_ContainerWithMount_CreatesVolume()
    {
        InstallPackage("docker");
        var container = Factory<Container>().Generate();
        var package = Factory<Package>().Generate() with
        {
            Contracts =
            [
                container with
                {
                    Mounts = new Dictionary<string, ContainerMount> { { "/mnt", new ContainerMount() } }
                }
            ]
        };

        var application = InstallPackage(package);
        Assert.True(application.Contracts.OfType<Volume>().Single().Installed);
    }


    [Fact]
    public void Install_ContainerWithSpecifiedApplication_InstallsCorrectNetworks()
    {
        var docker1 = InstallPackage("docker");
        var docker2 = InstallPackage("docker");
        var container = Factory<Container>().Generate();

        var application1 = InstallPackage(
            Factory<Package>().Generate() with { Contracts = [container with { HandlerApplication = docker1.Name }] }
        );
        var application2 = InstallPackage(
            Factory<Package>().Generate() with { Contracts = [container with { HandlerApplication = docker2.Name }] }
        );

        var network1 = application1.Contracts.OfType<Network>().Single();
        var network2 = application2.Contracts.OfType<Network>().Single();
        Assert.Equal(Handler<NetworkHandler>(docker1), network1.Handler);
        Assert.Equal(Handler<NetworkHandler>(docker2), network2.Handler);
        Assert.NotEqual(network1.Handler, network2.Handler);
    }

    [Fact]
    public void Install_ContainerWithSpecifiedApplication_InstallsCorrectVolumes()
    {
        var docker1 = InstallPackage("docker");
        var docker2 = InstallPackage("docker");
        var container = Factory<Container>().Generate() with
        {
            Mounts = new Dictionary<string, ContainerMount> { { "/mnt", new ContainerMount() } }
        };

        var application1 = InstallPackage(
            Factory<Package>().Generate() with { Contracts = [container with { HandlerApplication = docker1.Name }] }
        );
        var application2 = InstallPackage(
            Factory<Package>().Generate() with { Contracts = [container with { HandlerApplication = docker2.Name }] }
        );

        var volume1 = application1.Contracts.OfType<Volume>().Single();
        var volume2 = application2.Contracts.OfType<Volume>().Single();
        Assert.Equal(Handler<NewVolumeHandler>(docker1), volume1.Handler);
        Assert.Equal(Handler<NewVolumeHandler>(docker2), volume2.Handler);
        Assert.NotEqual(volume1.Handler, volume2.Handler);
    }
    
    [Fact]
    public void Install_ContainerWithSpecifiedApplication_InstallsCorrectPorts()
    {
        var docker1 = InstallPackage("docker");
        var docker2 = InstallPackage("docker");
        var container = Factory<Container>().Generate();
        var port = Factory<PortEndpoint>().Generate() with { Container = new ContractId<Container>(container.Name) };

        var application1 = InstallPackage(
            Factory<Package>().Generate() with { Contracts = [port, container with { HandlerApplication = docker1.Name }] }
        );
        var application2 = InstallPackage(
            Factory<Package>().Generate() with { Contracts = [port, container with { HandlerApplication = docker2.Name }] }
        );

        var port1 = application1.Contracts.OfType<PortEndpoint>().Single();
        var port2 = application2.Contracts.OfType<PortEndpoint>().Single();
        Assert.Equal(Handler<PortEndpointHandler>(docker1), port1.Handler);
        Assert.Equal(Handler<PortEndpointHandler>(docker2), port2.Handler);
        Assert.NotEqual(port1.Handler, port2.Handler);
    }
}