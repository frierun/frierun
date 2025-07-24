using Frierun.Server.Data;
using ContainerHandler = Frierun.Server.Handlers.Udocker.ContainerHandler;
using NetworkHandler = Frierun.Server.Handlers.Docker.NetworkHandler;

namespace Frierun.Tests.Handlers.Docker;

public class NetworkHandlerTests : BaseTests
{
    [Fact]
    public void Initialize_HavingSameUdockerNetworkName_DontDisturb()
    {
        var docker = InstallPackage("docker");
        var udocker = InstallPackage("termux-udocker");
        var container = Factory<Container>().Generate("udocker") with { Handler = Handler<ContainerHandler>(udocker) };
        var package = Factory<Package>().Generate() with { Contracts = [container] };
        var application = InstallPackage(package);
        var network = application.Contracts.OfType<Network>().Single();
        Assert.True(network.Installed);

        var result = Handler<NetworkHandler>(docker).Initialize(new Network("", NetworkName: network.NetworkName), "");

        var dockerNetwork = (Network)result.Single().Contract;
        Assert.Equal(network.NetworkName, dockerNetwork.NetworkName);
    }
}