using Bogus;
using Frierun.Server.Data;
using Frierun.Server.Handlers;
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
    public void Initialize_ContractWithMountDockerSocket_RefusesToInstall()
    {
        var container = Factory<Container>().Generate() with { MountDockerSocket = true };
        var handler = Handler<ContainerHandler>(_udocker);

        var result = handler.Initialize(container, new ApplicationContext("", ""));

        Assert.Empty(result);
    }

    [Fact]
    public void GetCommands_Container_ResolvesAllContainerArguments()
    {
        var parameter = Factory<Parameter>().Generate();
        var container = Factory<Container>().Generate("udocker") with
        {
            ImageName = new Argument<string>($"{{{{Parameter:{parameter.Name}:Value}}}}"),
            Env = new Dictionary<string, Argument<string>>
            {
                ["Test"] = new($"{{{{Parameter:{parameter.Name}:Value}}}}")
            }
        };
        var handler = Handler<ContainerHandler>(_udocker);
        var result = handler.Initialize(container, new ApplicationContext("", "")).Single();
        var daemon = result.Values.OfType<Daemon>().Single();
        container = (Container)container.Merge(result[container.Id]);
        var plan = new ExecutionPlan(
            new Dictionary<ContractId, Contract>
            {
                [container] = container,
                [parameter] = parameter,
            },
            []
        );

        Assert.False(container.ImageName.Resolved);
        Assert.False(container.Env.Values.Single().Resolved);

        daemon.Command.Resolve(plan);

        Assert.True(container.ImageName.Resolved);
        Assert.True(container.Env.Values.Single().Resolved);
    }

    [Fact]
    public void GetPreCommands_Container_ResolvesAllContainerArguments()
    {
        var parameter = Factory<Parameter>().Generate();
        var container = Factory<Container>().Generate("udocker") with
        {
            ImageName = new Argument<string>($"{{{{Parameter:{parameter.Name}:Value}}}}"),
            Env = new Dictionary<string, Argument<string>>
            {
                ["Test"] = new($"{{{{Parameter:{parameter.Name}:Value}}}}")
            }
        };
        var handler = Handler<ContainerHandler>(_udocker);
        var result = handler.Initialize(container, new ApplicationContext("", "")).Single();
        var daemon = result.Values.OfType<Daemon>().Single();
        container = (Container)container.Merge(result[container.Id]);
        var plan = new ExecutionPlan(
            new Dictionary<ContractId, Contract>
            {
                [container] = container,
                [parameter] = parameter,
            },
            []
        );

        Assert.False(container.ImageName.Resolved);
        Assert.False(container.Env.Values.Single().Resolved);

        daemon.PreCommands.Resolve(plan);

        Assert.True(container.ImageName.Resolved);
        Assert.True(container.Env.Values.Single().Resolved);
    }


    [Fact]
    public void Install_Container_CreatesDaemon()
    {
        var container = Factory<Container>().Generate("udocker");
        var package = Factory<Package>().Generate() with { Contracts = [container] };

        var application = InstallPackage(package);

        var daemon = application.GetContracts<Daemon>().Single();
        Assert.NotNull(daemon.Command.Value);
        Assert.Contains("udocker", daemon.Command.Value);
        Assert.Contains(container.ContainerName, daemon.Command.Value);

        Assert.NotNull(daemon.PreCommands.Value);
        var preCommand =
            daemon.PreCommands.Value
                .Single(command =>
                    {
                        var enumerable = command.ToList();
                        return enumerable.Contains("udocker") && enumerable.Contains("create");
                    }
                )
                .ToList();
        Assert.Contains($"--name={container.ContainerName}", preCommand);
        Assert.Contains(container.ImageName.Value, preCommand);
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

        var volume = application.GetContracts<Volume>().Single();
        var daemon = application.GetContracts<Daemon>().Single();
        Assert.NotNull(daemon.Command.Value);
        Assert.Contains($"--volume={volume.LocalPath}:/test", daemon.Command.Value);

        Assert.NotNull(daemon.PreCommands.Value);
        var preCommand = daemon.PreCommands.Value.Single(command => command.Contains("mkdir"));
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
                new PortEndpoint(Protocol.Tcp, 80, Container: new ContractId<Container>(container.Id))
            ]
        };

        var application = InstallPackage(package);

        var portEndpoint = application.GetContracts<PortEndpoint>().Single();
        var daemon = application.GetContracts<Daemon>().Single();
        Assert.NotNull(daemon.Command.Value);
        Assert.Contains($"--publish={portEndpoint.ExternalPort}:80", daemon.Command.Value);
    }

    [Fact]
    public void Install_ContainerWithEnv_PassesEnv()
    {
        var name = Resolve<Faker>().Lorem.Word();
        var value = Resolve<Faker>().Lorem.Word();

        var container = Factory<Container>().Generate("udocker") with
        {
            Env = new Dictionary<string, Argument<string>>
            {
                { name, value }
            }
        };
        var package = Factory<Package>().Generate() with { Contracts = [container] };

        var application = InstallPackage(package);

        var daemon = application.GetContracts<Daemon>().Single();
        Assert.NotNull(daemon.Command.Value);
        Assert.Contains($"--env={name}={value}", daemon.Command.Value);
    }

    [Fact]
    public void Install_ContainerWithTemplateEnv_EvaluatesEnv()
    {
        var name = Resolve<Faker>().Lorem.Word();
        var value = Resolve<Faker>().Lorem.Word();

        var package = Factory<Package>().Generate() with
        {
            Contracts =
            [
                Factory<Container>().Generate("udocker") with
                {
                    Env = new Dictionary<string, Argument<string>> { { name, "{{Parameter:Test:Value}}" } }
                },
                new Parameter("Test", Value: value)
            ]
        };

        var application = InstallPackage(package);

        var daemon = application.GetContracts<Daemon>().Single();
        Assert.NotNull(daemon.Command.Value);
        Assert.Contains($"--env={name}={value}", daemon.Command.Value);
    }

    [Fact]
    public void Install_ContainerWithChanges_AppliesChanges()
    {
        var name = Resolve<Faker>().Lorem.Word();
        var value = Resolve<Faker>().Lorem.Word();
        var container = Factory<Container>().Generate("udocker");
        var package = Factory<Package>().Generate() with
        {
            Contracts =
            [
                container,
                new Selector(
                    "test", new List<SelectorOption>
                    {
                        new(
                            "one",
                            [
                                new Container(container.Name)
                                {
                                    Env = new Dictionary<string, Argument<string>>
                                    {
                                        { name, value }
                                    }
                                }
                            ]
                        )
                    }
                )
            ]
        };

        var application = InstallPackage(package);

        var daemon = application.GetContracts<Daemon>().Single();
        Assert.NotNull(daemon.Command.Value);
        Assert.Contains($"--env={name}={value}", daemon.Command.Value);
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

        var network1 = application1.GetContracts<Network>().Single();
        var network2 = application2.GetContracts<Network>().Single();
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

        var volume1 = application1.GetContracts<Volume>().Single();
        var volume2 = application2.GetContracts<Volume>().Single();
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
        var port = Factory<PortEndpoint>().Generate() with { Container = new ContractId<Container>(container.Id) };

        var application1 = InstallPackage(
            Factory<Package>().Generate() with
            {
                Contracts = [port, container with { HandlerApplication = udocker1.Name }]
            }
        );
        var application2 = InstallPackage(
            Factory<Package>().Generate() with
            {
                Contracts = [port, container with { HandlerApplication = udocker2.Name }]
            }
        );

        var port1 = application1.GetContracts<PortEndpoint>().Single();
        var port2 = application2.GetContracts<PortEndpoint>().Single();
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

        var daemon1 = application1.GetContracts<Daemon>().Single();
        var daemon2 = application2.GetContracts<Daemon>().Single();
        Assert.Equal(Handler<DaemonHandler>(udocker1), daemon1.Handler);
        Assert.Equal(Handler<DaemonHandler>(udocker2), daemon2.Handler);
        Assert.NotEqual(daemon1.Handler, daemon2.Handler);
    }
}