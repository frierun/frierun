using Frierun.Server.Data;
using Frierun.Server.Handlers;
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
        var package = Factory<Package>().Generate() with
        {
            Contracts = [Contract<Container>().SetHandler<ContainerHandler>(udocker).Generate("udocker")]
        };
        var application = InstallPackage(package);
        var network = State.GetContract<Network>(application);
        Assert.True(network.Installed);

        var result = Handler<NetworkHandler>(docker)
            .Initialize(
                new Network(),
                new ApplicationContext("", network.NetworkName)
            );

        var dockerNetwork = result.Single().Values.OfType<Network>().Single();
        Assert.Equal(network.NetworkName, dockerNetwork.NetworkName);
    }
}