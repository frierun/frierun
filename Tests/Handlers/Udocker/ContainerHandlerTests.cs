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
    public void GetCommands_Container_ResolvesEnvArguments()
    {
        var value = Resolve<Faker>().Lorem.Word();
        var parameter = Contract<Parameter>().Set(p => p.Value, value).Generate();
        var container = Contract<Container>()
            .Set(p => p.ImageName, $"{{{{{parameter.Ref}:Value}}}}")
            .Set(p => p.Env, new Dictionary<string, Argument<string>> { ["Test"] = $"{{{{{parameter.Ref}:Value}}}}" })
            .Generate("udocker");
        var handler = Handler<ContainerHandler>(_udocker);
        var result = handler.Initialize(container.Contract, new ApplicationContext(container.Ref, "")).Single();
        var daemon = result.Values.OfType<Daemon>().Single();
        container = container with { Contract = (Container)container.Contract.Merge(result[container.Ref]) };
        var plan = new ExecutionPlan(
            new Dictionary<ContractRef, Contract>
            {
                { container.Ref, container.Contract },
                { new ContractRef<Network>(), new Network() },
                { parameter.Ref, parameter.Contract },
                { new ContractRef<Daemon>(container.Ref.Name), daemon}
            },
            []
        );

        Assert.False(container.Contract.Env.Values.Single().Resolved);
        
        var command = daemon.Command.Resolve(plan);

        Assert.True(command.Resolved);
        Assert.NotNull(command.Value);
        Assert.Contains($"--env=Test={value}", command.Value);
    }

    [Fact]
    public void GetPreCommands_Container_ResolvesImageNameArgument()
    {
        var value = Resolve<Faker>().Lorem.Word();
        var parameter = Contract<Parameter>().Set(p => p.Value, value).Generate();
        var container = Contract<Container>()
            .Set(p => p.ImageName, $"{{{{{parameter.Ref}:Value}}}}")
            .Set(p => p.Env, new Dictionary<string, Argument<string>> { ["Test"] = $"{{{{{parameter.Ref}:Value}}}}" })
            .Generate("udocker");
        var handler = Handler<ContainerHandler>(_udocker);
        var result = handler.Initialize(container.Contract, new ApplicationContext(container.Ref, "")).Single();
        var daemon = result.Values.OfType<Daemon>().Single();
        container = container with { Contract = (Container)container.Contract.Merge(result[container.Ref]) };
        var plan = new ExecutionPlan(
            new Dictionary<ContractRef, Contract>
            {
                { container.Ref, container.Contract },
                { new ContractRef<Network>(), new Network() },
                { parameter.Ref, parameter.Contract },
                { new ContractRef<Daemon>(container.Ref.Name), daemon}
            },
            []
        );

        Assert.False(container.Contract.ImageName.Resolved);

        var preCommands = daemon.PreCommands.Resolve(plan);

        Assert.True(preCommands.Resolved);
        Assert.NotNull(preCommands.Value);
        Assert.Contains(value, preCommands.Value.SelectMany(command => command));
    }


    [Fact]
    public void Install_Container_CreatesDaemon()
    {
        var container = Contract<Container>().Generate("udocker");
        var package = Factory<Package>().Generate() with { Contracts = [container] };

        var application = InstallPackage(package);

        var daemon = State.GetContract<Daemon>(application, container.Ref.Name);
        Assert.NotNull(daemon.Command.Value);
        Assert.Contains("udocker", daemon.Command.Value);
        Assert.Contains(container.Contract.ContainerName, daemon.Command.Value);

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
        Assert.Contains($"--name={container.Contract.ContainerName}", preCommand);
        Assert.Contains(container.Contract.ImageName.Value, preCommand);
    }

    [Fact]
    public void Install_ContainerWithVolume_CreatesPath()
    {
        var container = Contract<Container>()
            .Set(p => p.Mounts, new Dictionary<string, ContainerMount> { { "/test", new ContainerMount() } })
            .Generate("udocker");
        var package = Factory<Package>().Generate() with { Contracts = [container] };

        var application = InstallPackage(package);

        var volume = State.GetContract<Volume>(application);
        var daemon = State.GetContract<Daemon>(application, container.Ref.Name);
        Assert.NotNull(daemon.Command.Value);
        Assert.Contains($"--volume={volume.LocalPath}:/test", daemon.Command.Value);

        Assert.NotNull(daemon.PreCommands.Value);
        var preCommand = daemon.PreCommands.Value.Single(command => command.Contains("mkdir"));
        Assert.Contains(volume.LocalPath, preCommand);
    }

    [Fact]
    public void Install_ContainerWithPort_PublishesPort()
    {
        var container = Contract<Container>().Generate("udocker");
        var portEndpoint = Contract<PortEndpoint>()
            .Set(p => p.Protocol, Protocol.Tcp)
            .Set(p => p.Container, container.Id)
            .Generate();
        var package = Factory<Package>().Generate() with { Contracts = [container, portEndpoint] };

        var application = InstallPackage(package);

        var installedPortEndpoint = State.GetContract(application, portEndpoint.Ref);
        Assert.True(installedPortEndpoint.Installed);
        var daemon = State.GetContract<Daemon>(application, container.Ref.Name);
        Assert.NotNull(daemon.Command.Value);
        Assert.Contains(
            $"--publish={installedPortEndpoint.ExternalPort}:{installedPortEndpoint.Port}",
            daemon.Command.Value
        );
    }

    [Fact]
    public void Install_ContainerWithEnv_PassesEnv()
    {
        var name = Resolve<Faker>().Lorem.Word();
        var value = Resolve<Faker>().Lorem.Word();
        var container = Contract<Container>()
            .Set(p => p.Env, new Dictionary<string, Argument<string>> { [name] = value })
            .Generate("udocker");
        var package = Factory<Package>().Generate() with { Contracts = [container] };

        var application = InstallPackage(package);

        var daemon = State.GetContract<Daemon>(application, container.Ref.Name);
        Assert.NotNull(daemon.Command.Value);
        Assert.Contains($"--env={name}={value}", daemon.Command.Value);
    }

    [Fact]
    public void Install_ContainerWithTemplateEnv_EvaluatesEnv()
    {
        var name = Resolve<Faker>().Lorem.Word();
        var value = Resolve<Faker>().Lorem.Word();

        var parameter = Contract<Parameter>().Set(p => p.Value, value).Generate();
        var container = Contract<Container>()
            .Set(p => p.Env, new Dictionary<string, Argument<string>> { [name] = $"{{{{{parameter.Ref}:Value}}}}" })
            .Generate("udocker");
        var package = Factory<Package>().Generate() with
        {
            Contracts = [container, parameter]
        };

        var application = InstallPackage(package);

        var daemon = State.GetContract<Daemon>(application, container.Ref.Name);
        Assert.NotNull(daemon.Command.Value);
        Assert.Contains($"--env={name}={value}", daemon.Command.Value);
    }

    [Fact]
    public void Install_ContainerWithChanges_AppliesChanges()
    {
        var name = Resolve<Faker>().Lorem.Word();
        var value = Resolve<Faker>().Lorem.Word();
        var container = Contract<Container>().Generate("udocker");
        var selector = Contract<Selector>()
            .Set(
                p => p.Options,
                [
                    new SelectorOption(
                        "one",
                        [
                            container.With(c =>
                                c with { Env = new Dictionary<string, Argument<string>> { { name, value } } }
                            )
                        ]
                    )
                ]
            )
            .Generate();
        var package = Factory<Package>().Generate() with
        {
            Contracts = [container, selector]
        };

        var application = InstallPackage(package);

        var daemon = State.GetContract<Daemon>(application, container.Ref.Name);
        Assert.NotNull(daemon.Command.Value);
        Assert.Contains($"--env={name}={value}", daemon.Command.Value);
    }

    [Fact]
    public void Install_ContainerWithSpecifiedApplication_InstallsCorrectNetworks()
    {
        var udocker1 = InstallPackage("termux-udocker");
        var udocker2 = InstallPackage("termux-udocker");
        var container = Contract<Container>().Generate("udocker");

        var application1 = InstallPackage(
            Factory<Package>().Generate() with
            {
                Contracts = [container.With(c => c with { HandlerApplication = udocker1.Name })]
            }
        );
        var application2 = InstallPackage(
            Factory<Package>().Generate() with
            {
                Contracts = [container.With(c => c with { HandlerApplication = udocker2.Name })]
            }
        );

        var network1 = State.GetContract<Network>(application1);
        var network2 = State.GetContract<Network>(application2);
        Assert.Equal(Handler<NetworkHandler>(udocker1), network1.Handler);
        Assert.Equal(Handler<NetworkHandler>(udocker2), network2.Handler);
        Assert.NotEqual(network1.Handler, network2.Handler);
    }

    [Fact]
    public void Install_ContainerWithSpecifiedApplication_InstallsCorrectVolumes()
    {
        var udocker1 = InstallPackage("termux-udocker");
        var udocker2 = InstallPackage("termux-udocker");
        var container = Contract<Container>()
            .Set(p => p.Mounts, new Dictionary<string, ContainerMount> { { "/mnt", new ContainerMount() } })
            .Generate("udocker");

        var application1 = InstallPackage(
            Factory<Package>().Generate() with
            {
                Contracts = [container.With(c => c with { HandlerApplication = udocker1.Name })]
            }
        );
        var application2 = InstallPackage(
            Factory<Package>().Generate() with
            {
                Contracts = [container.With(c => c with { HandlerApplication = udocker2.Name })]
            }
        );

        var volume1 = State.GetContract<Volume>(application1);
        var volume2 = State.GetContract<Volume>(application2);
        Assert.Equal(Handler<LocalPathHandler>(udocker1), volume1.Handler);
        Assert.Equal(Handler<LocalPathHandler>(udocker2), volume2.Handler);
        Assert.NotEqual(volume1.Handler, volume2.Handler);
    }

    [Fact]
    public void Install_ContainerWithSpecifiedApplication_InstallsCorrectPorts()
    {
        var udocker1 = InstallPackage("termux-udocker");
        var udocker2 = InstallPackage("termux-udocker");
        var container = Contract<Container>().Generate("udocker");
        var portEndpoint = Contract<PortEndpoint>().Set(p => p.Container, container.Id).Generate("udocker");

        var application1 = InstallPackage(
            Factory<Package>().Generate() with
            {
                Contracts =
                [
                    portEndpoint,
                    container.With(c => c with { HandlerApplication = udocker1.Name })
                ]
            }
        );
        var application2 = InstallPackage(
            Factory<Package>().Generate() with
            {
                Contracts =
                [
                    portEndpoint,
                    container.With(c => c with { HandlerApplication = udocker2.Name })
                ]
            }
        );

        var port1 = State.GetContract(application1, portEndpoint.Ref);
        var port2 = State.GetContract(application2, portEndpoint.Ref);
        Assert.Equal(Handler<PortEndpointHandler>(udocker1), port1.Handler);
        Assert.Equal(Handler<PortEndpointHandler>(udocker2), port2.Handler);
        Assert.NotEqual(port1.Handler, port2.Handler);
    }

    [Fact]
    public void Install_ContainerWithSpecifiedApplication_InstallsCorrectDaemons()
    {
        var udocker1 = InstallPackage("termux-udocker");
        var udocker2 = InstallPackage("termux-udocker");
        var container = Contract<Container>().Generate("udocker");

        var application1 = InstallPackage(
            Factory<Package>().Generate() with
            {
                Contracts = [container.With(c => c with { HandlerApplication = udocker1.Name })]
            }
        );
        var application2 = InstallPackage(
            Factory<Package>().Generate() with
            {
                Contracts = [container.With(c => c with { HandlerApplication = udocker2.Name })]
            }
        );

        var daemon1 = State.GetContract<Daemon>(application1, container.Ref.Name);
        var daemon2 = State.GetContract<Daemon>(application2, container.Ref.Name);
        Assert.Equal(Handler<DaemonHandler>(udocker1), daemon1.Handler);
        Assert.Equal(Handler<DaemonHandler>(udocker2), daemon2.Handler);
        Assert.NotEqual(daemon1.Handler, daemon2.Handler);
    }
}