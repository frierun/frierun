using Frierun.Server.Data;
using Frierun.Server.Handlers.Udocker;

namespace Frierun.Tests.Handlers.Udocker;

public class ContainerHandlerTests : BaseTests
{
    private readonly Application _udocker;

    public ContainerHandlerTests()
    {
        _udocker = InstallPackage("termux-udocker");
    }

    [Fact]
    public void Initialize_ContractWithMountDockerSocket_ReturnsEmpty()
    {
        var container = Factory<Container>().Generate() with { MountDockerSocket = true };
        var handler = Handler<ContainerHandler>(_udocker);

        var result = handler.Initialize(container, "");

        Assert.Empty(result);
    }

    [Fact]
    public void Install_Container_CreatesDaemon()
    {
        var container = Factory<Container>().Generate("udocker");
        var package = Factory<Package>().Generate() with { Contracts = [container] };

        var application = InstallPackage(package);

        var daemon = application.Contracts.OfType<Daemon>().Single();
        Assert.Contains("udocker", daemon.Command);
        Assert.Contains(container.ContainerName, daemon.Command);

        var preCommand =
            daemon.PreCommands.Single(command => command.Contains("udocker") && command.Contains("create"));
        Assert.Contains($"--name={container.ContainerName}", preCommand);
        Assert.Contains(container.ImageName, preCommand);
    }

    [Fact]
    public void Install_ContainerWithVolume_CreatesPath()
    {
        var container = Factory<Container>().Generate("udocker") with
        {
            Mounts = new Dictionary<string, ContainerMount> { { "/test", new ContainerMount() } }
        };
        var package = Factory<Package>().Generate() with { Contracts = [container] };

        var application = InstallPackage(package);

        var volume = application.Contracts.OfType<Volume>().Single();
        var daemon = application.Contracts.OfType<Daemon>().Single();
        Assert.Contains($"--volume={volume.LocalPath}:/test", daemon.Command);

        var preCommand = daemon.PreCommands.Single(command => command.Contains("mkdir"));
        Assert.Contains(volume.LocalPath, preCommand);
    }

    [Fact]
    public void Install_ContainerWithPort_PublishesPort()
    {
        var container = Factory<Container>().Generate("udocker");
        var package = Factory<Package>().Generate() with
        {
            Contracts =
            [
                container,
                new PortEndpoint(Protocol.Tcp, 80, Container: new ContractId<Container>(container.Name))
            ]
        };

        var application = InstallPackage(package);

        var portEndpoint = application.Contracts.OfType<PortEndpoint>().Single();
        var daemon = application.Contracts.OfType<Daemon>().Single();
        Assert.Contains($"--publish={portEndpoint.ExternalPort}:80", daemon.Command);
    }

    [Fact]
    public void Install_ContainerWithEnv_PassesEnv()
    {
        var container = Factory<Container>().Generate("udocker") with
        {
            Env = new Dictionary<string, string>
            {
                { "name", "value" }
            }
        };
        var package = Factory<Package>().Generate() with { Contracts = [container] };

        var application = InstallPackage(package);

        var daemon = application.Contracts.OfType<Daemon>().Single();
        Assert.Contains($"--env=name=value", daemon.Command);
    }
    
    [Fact]
    public void Install_ContainerWithSpecifiedApplication_InstallsCorrectNetworks()
    {
        var udocker1 = InstallPackage("termux-udocker");
        var udocker2 = InstallPackage("termux-udocker");
        var container = Factory<Container>().Generate("udocker");

        var application1 = InstallPackage(
            Factory<Package>().Generate() with { Contracts = [container with { HandlerApplication = udocker1.Name }] }
        );
        var application2 = InstallPackage(
            Factory<Package>().Generate() with { Contracts = [container with { HandlerApplication = udocker2.Name }] }
        );

        var network1 = application1.Contracts.OfType<Network>().Single();
        var network2 = application2.Contracts.OfType<Network>().Single();
        Assert.Equal(Handler<NetworkHandler>(udocker1), network1.Handler);
        Assert.Equal(Handler<NetworkHandler>(udocker2), network2.Handler);
        Assert.NotEqual(network1.Handler, network2.Handler);
    }

    [Fact]
    public void Install_ContainerWithSpecifiedApplication_InstallsCorrectVolumes()
    {
        var udocker1 = InstallPackage("termux-udocker");
        var udocker2 = InstallPackage("termux-udocker");
        var container = Factory<Container>().Generate("udocker") with
        {
            Mounts = new Dictionary<string, ContainerMount> { { "/mnt", new ContainerMount() } }
        };

        var application1 = InstallPackage(
            Factory<Package>().Generate() with { Contracts = [container with { HandlerApplication = udocker1.Name }] }
        );
        var application2 = InstallPackage(
            Factory<Package>().Generate() with { Contracts = [container with { HandlerApplication = udocker2.Name }] }
        );

        var volume1 = application1.Contracts.OfType<Volume>().Single();
        var volume2 = application2.Contracts.OfType<Volume>().Single();
        Assert.Equal(Handler<LocalPathHandler>(udocker1), volume1.Handler);
        Assert.Equal(Handler<LocalPathHandler>(udocker2), volume2.Handler);
        Assert.NotEqual(volume1.Handler, volume2.Handler);
    }
    
    [Fact]
    public void Install_ContainerWithSpecifiedApplication_InstallsCorrectPorts()
    {
        var udocker1 = InstallPackage("termux-udocker");
        var udocker2 = InstallPackage("termux-udocker");
        var container = Factory<Container>().Generate("udocker");
        var port = Factory<PortEndpoint>().Generate() with { Container = new ContractId<Container>(container.Name) };

        var application1 = InstallPackage(
            Factory<Package>().Generate() with { Contracts = [port, container with { HandlerApplication = udocker1.Name }] }
        );
        var application2 = InstallPackage(
            Factory<Package>().Generate() with { Contracts = [port, container with { HandlerApplication = udocker2.Name }] }
        );

        var port1 = application1.Contracts.OfType<PortEndpoint>().Single();
        var port2 = application2.Contracts.OfType<PortEndpoint>().Single();
        Assert.Equal(Handler<PortEndpointHandler>(udocker1), port1.Handler);
        Assert.Equal(Handler<PortEndpointHandler>(udocker2), port2.Handler);
        Assert.NotEqual(port1.Handler, port2.Handler);
    }
    
    [Fact]
    public void Install_ContainerWithSpecifiedApplication_InstallsCorrectDaemons()
    {
        var udocker1 = InstallPackage("termux-udocker");
        var udocker2 = InstallPackage("termux-udocker");
        var container = Factory<Container>().Generate("udocker");

        var application1 = InstallPackage(
            Factory<Package>().Generate() with { Contracts = [container with { HandlerApplication = udocker1.Name }] }
        );
        var application2 = InstallPackage(
            Factory<Package>().Generate() with { Contracts = [container with { HandlerApplication = udocker2.Name }] }
        );

        var daemon1 = application1.Contracts.OfType<Daemon>().Single();
        var daemon2 = application2.Contracts.OfType<Daemon>().Single();
        Assert.Equal(Handler<DaemonHandler>(udocker1), daemon1.Handler);
        Assert.Equal(Handler<DaemonHandler>(udocker2), daemon2.Handler);
        Assert.NotEqual(daemon1.Handler, daemon2.Handler);
    }
    
}