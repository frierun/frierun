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
        var container = Factory<Container>().Generate() with { MountDockerSocket = false };
        var package = Factory<Package>().Generate() with { Contracts = [container] };

        var application = InstallPackage(package);

        var daemon = application.Contracts.OfType<Daemon>().Single();
        Assert.Contains("udocker", daemon.Command);
        Assert.Contains("--name=" + container.ContainerName, daemon.Command);
        Assert.Contains(container.ImageName, daemon.Command);
    }

    [Fact]
    public void Install_ContainerWithVolume_CreatesPath()
    {
        var container = Factory<Container>().Generate() with
        {
            MountDockerSocket = false,
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
        var container = Factory<Container>().Generate() with
        {
            MountDockerSocket = false,
        };
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
        var container = Factory<Container>().Generate() with
        {
            MountDockerSocket = false,
            Env = new Dictionary<string, string>
            {
                {"name", "value"}
            }
        };
        var package = Factory<Package>().Generate() with { Contracts = [container] };

        var application = InstallPackage(package);

        var daemon = application.Contracts.OfType<Daemon>().Single();
        Assert.Contains($"--env=\"name=value\"", daemon.Command);
    }
}