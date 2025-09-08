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
            Contracts = new ContractList
            {
                Contract<Container>().Generate("udocker")
                    .With(c => c with { Handler = Handler<ContainerHandler>(udocker) })
            }
        };
        var application = InstallPackage(package);
        var network = application.GetContracts<Network>().Single();
        Assert.True(network.Installed);

        var result = Handler<NetworkHandler>(docker)
            .Initialize(
                new Network(""),
                new ApplicationContext("", network.NetworkName)
            );

        var dockerNetwork = (Network)result.Single()[network.Id];
        Assert.Equal(network.NetworkName, dockerNetwork.NetworkName);
    }
}