using Docker.DotNet.Models;
using Frierun.Server.Data;
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
}