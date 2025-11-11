using Bogus;
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
        var container = Contract<Container>().Generate();
        var package = Factory<Package>().Generate() with { Contracts = [container] };

        var application = InstallPackage(package);

        Assert.True(State.GetContract<Network>(application).Installed);
    }

    [Fact]
    public void Install_RequireDocker_MountsSocket()
    {
        InstallPackage("docker");
        var container = Contract<Container>().Set<bool?>(p => p.MountDockerSocket, true).Generate();
        var package = Factory<Package>().Generate() with { Contracts = [container] };

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
        const string path = "/run/podman/podman.sock";
        Handler<FakeDockerApiConnectionHandler>().SocketRootPath = path;
        InstallPackage("docker");
        var container = Contract<Container>().Set<bool?>(p => p.MountDockerSocket, true).Generate();
        var package = Factory<Package>().Generate() with { Contracts = [container] };

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
        var container = Contract<Container>()
            .Set(p => p.Mounts, new Dictionary<string, ContainerMount> { { "/mnt", new ContainerMount() } })
            .Generate();
        var package = Factory<Package>().Generate() with { Contracts = [container] };

        var application = InstallPackage(package);

        Assert.True(State.GetContract<Volume>(application).Installed);
    }

    [Fact]
    public void Install_ContainerWithSpecifiedApplication_InstallsCorrectNetworks()
    {
        var docker1 = InstallPackage("docker");
        var docker2 = InstallPackage("docker");
        var container = Contract<Container>().Generate();

        var application1 = InstallPackage(
            Factory<Package>().Generate() with
            {
                Contracts = [container.With(c => c with { HandlerApplication = docker1.Name })]
            }
        );
        var application2 = InstallPackage(
            Factory<Package>().Generate() with
            {
                Contracts = [container.With(c => c with { HandlerApplication = docker2.Name })]
            }
        );

        var network1 = State.GetContract<Network>(application1);
        var network2 = State.GetContract<Network>(application2);
        Assert.Equal(Handler<NetworkHandler>(docker1), network1.Handler);
        Assert.Equal(Handler<NetworkHandler>(docker2), network2.Handler);
        Assert.NotEqual(network1.Handler, network2.Handler);
    }

    [Fact]
    public void Install_ContainerWithSpecifiedApplication_InstallsCorrectVolumes()
    {
        var docker1 = InstallPackage("docker");
        var docker2 = InstallPackage("docker");
        var container = Contract<Container>()
            .Set(p => p.Mounts, new Dictionary<string, ContainerMount> { { "/mnt", new ContainerMount() } })
            .Generate();

        var application1 = InstallPackage(
            Factory<Package>().Generate() with
            {
                Contracts = [container.With(c => c with { HandlerApplication = docker1.Name })]
            }
        );
        var application2 = InstallPackage(
            Factory<Package>().Generate() with
            {
                Contracts = [container.With(c => c with { HandlerApplication = docker2.Name })]
            }
        );

        var volume1 = State.GetContract<Volume>(application1);
        var volume2 = State.GetContract<Volume>(application2);
        Assert.Equal(Handler<VolumeHandler>(docker1), volume1.Handler);
        Assert.Equal(Handler<VolumeHandler>(docker2), volume2.Handler);
        Assert.NotEqual(volume1.Handler, volume2.Handler);
    }

    [Fact]
    public void Install_ContainerWithSpecifiedApplication_InstallsCorrectPorts()
    {
        var docker1 = InstallPackage("docker");
        var docker2 = InstallPackage("docker");
        var container = Contract<Container>().Generate();
        var portEndpoint = Contract<PortEndpoint>().Set(p => p.Container, container.Ref).Generate();

        var application1 = InstallPackage(
            Factory<Package>().Generate() with
            {
                Contracts = [portEndpoint, container.With(c => c with { HandlerApplication = docker1.Name })]
            }
        );
        var application2 = InstallPackage(
            Factory<Package>().Generate() with
            {
                Contracts = [portEndpoint, container.With(c => c with { HandlerApplication = docker2.Name })]
            }
        );

        var port1 = State.GetContract(application1, portEndpoint.Ref);
        var port2 = State.GetContract(application2, portEndpoint.Ref);
        Assert.Equal(Handler<PortEndpointHandler>(docker1), port1.Handler);
        Assert.Equal(Handler<PortEndpointHandler>(docker2), port2.Handler);
        Assert.NotEqual(port1.Handler, port2.Handler);
    }

    [Fact]
    public void Install_ContainerWithEnv_PassesEnv()
    {
        var name = Resolve<Faker>().Lorem.Word();
        var value = Resolve<Faker>().Lorem.Word();
        InstallPackage("docker");
        var container = Contract<Container>()
            .Set(p => p.Env, new Dictionary<string, Argument<string>> { [name] = value })
            .Generate();

        var package = Factory<Package>().Generate() with
        {
            Contracts = [container]
        };

        InstallPackage(package);

        DockerClient.Containers.Received(1)
            .CreateContainerAsync(
                Arg.Is<CreateContainerParameters>(p =>
                    p.Env.Contains($"{name}={value}")
                )
            );
    }

    [Fact]
    public void Install_ContainerWithTemplateEnv_EvaluatesEnv()
    {
        var name = Resolve<Faker>().Lorem.Word();
        var value = Resolve<Faker>().Lorem.Word();
        InstallPackage("docker");
        var parameter = Contract<Parameter>().Set(p => p.Value, value).Generate();
        var container = Contract<Container>()
            .Set(p => p.Env, new Dictionary<string, Argument<string>> { [name] = new($"{{{{{parameter.Ref}:Value}}}}") })
            .Generate();
        var package = Factory<Package>().Generate() with
        {
            Contracts = [parameter, container]
        };

        InstallPackage(package);

        DockerClient.Containers.Received(1)
            .CreateContainerAsync(
                Arg.Is<CreateContainerParameters>(p =>
                    p.Env.Contains($"{name}={value}")
                )
            );
    }
}