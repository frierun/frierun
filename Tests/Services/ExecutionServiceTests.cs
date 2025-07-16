using Frierun.Server;
using Frierun.Server.Data;
using Frierun.Server.Handlers;
using NSubstitute;
using Substitute = NSubstitute.Substitute;

namespace Frierun.Tests;

public class ExecutionServiceTests : BaseTests
{
    public record Contract1(string? Name = null) : Contract(Name ?? "")
    {
        public override Contract Merge(Contract other)
        {
            return this;
        }
    }

    public record Contract2(string? Name = null) : Contract(Name ?? "")
    {
        public override Contract Merge(Contract other)
        {
            throw new NotImplementedException();
        }
    }

    private ExecutionService Service => Resolve<ExecutionService>();

    [Fact]
    public void Create_EmptyPackage_ReturnsPlan()
    {
        var package = Factory<Package>().Generate();

        var plan = Service.Create(package);

        Assert.NotNull(plan);
        Assert.Single(plan.Contracts);
        Assert.NotNull(plan.GetContract(package));
    }

    [Fact]
    public void Create_WithoutHandler_ThrowsException()
    {
        var contract = Substitute.For<Contract>("", false, null, null, null);
        var package = Factory<Package>().Generate() with { Contracts = [contract] };

        Assert.Throws<HandlerNotFoundException>(() => Service.Create(package));
    }

    [Fact]
    public void Create_HandlerWithoutOptions_ThrowsException()
    {
        var handler = Mock<Handler<Contract1>, IHandler>([null]);

        var contract = new Contract1();
        var package = Factory<Package>().Generate() with { Contracts = [contract] };
        handler
            .Initialize(Arg.Any<Contract1>(), Arg.Any<string>())
            .Returns([]);

        Assert.Throws<HandlerNotFoundException>(() => Service.Create(package));
    }

    [Fact]
    public void Create_HandlerReturnsUnknownContract_ThrowsException()
    {
        var handler = Mock<Handler<Contract1>, IHandler>([null]);

        var contract = new Contract1();
        var unknownContract = new Contract2();
        var package = Factory<Package>().Generate() with { Contracts = [contract] };
        handler
            .Initialize(Arg.Any<Contract1>(), Arg.Any<string>())
            .Returns([new ContractInitializeResult(contract with { Handler = handler }, [unknownContract])]);

        Assert.Throws<HandlerNotFoundException>(() => Service.Create(package));
    }

    [Fact]
    public void Create_CorrectHandler_ExecutesInitialize()
    {
        var handler = Mock<Handler<Contract1>, IHandler>([null]);

        var contract = new Contract1();
        var package = Factory<Package>().Generate() with { Contracts = [contract] };
        handler
            .Initialize(Arg.Any<Contract1>(), Arg.Any<string>())
            .Returns([new ContractInitializeResult(contract with { Handler = handler })]);

        var plan = Service.Create(package);

        Assert.NotNull(plan);
        Assert.Equal(2, plan.Contracts.Count());
        Assert.NotNull(plan.GetContract(package));
        Assert.NotNull(plan.GetContract(contract));
        // ReSharper disable once IteratorMethodResultIsIgnored
        handler.Received(1).Initialize(Arg.Any<Contract1>(), Arg.Any<string>());
    }

    [Fact]
    public void Create_RecursiveHandler_InitializesPlan()
    {
        var handler = Mock<Handler<Contract1>, IHandler>([null]);

        var contract = new Contract1();
        var package = Factory<Package>().Generate() with { Contracts = [contract] };
        handler
            .Initialize(Arg.Any<Contract1>(), Arg.Any<string>())
            .Returns([new ContractInitializeResult(contract with { Handler = handler }, [contract])]);

        var plan = Service.Create(package);

        Assert.NotNull(plan);
        Assert.Equal(2, plan.Contracts.Count());
        Assert.NotNull(plan.GetContract(package));
        Assert.NotNull(plan.GetContract(contract));
    }

    [Fact]
    public void Create_HandlerWithTwoBranches_InitializesPlan()
    {
        var handler = Mock<Handler<Contract1>, IHandler>([null]);

        var contract = new Contract1();
        var unknownContract = new Contract2();
        var knownContract = new Contract1("second");
        var package = Factory<Package>().Generate() with { Contracts = [contract] };
        handler
            .Initialize(Arg.Any<Contract1>(), Arg.Any<string>())
            .Returns(info =>
                [
                    new ContractInitializeResult(info.Arg<Contract1>() with { Handler = handler }, [unknownContract]),
                    new ContractInitializeResult(info.Arg<Contract1>() with { Handler = handler }, [knownContract])
                ]
            );

        var plan = Service.Create(package);

        Assert.NotNull(plan);
        Assert.Equal(3, plan.Contracts.Count());
        Assert.NotNull(plan.GetContract(package));
        Assert.NotNull(plan.GetContract(contract));
        Assert.NotNull(plan.GetContract(knownContract));
    }

    [Fact]
    public void Create_TwoHandlers_InitializesPlan()
    {
        var handler = Mock<Handler<Contract1>, IHandler>([null]);
        var handler2 = Mock<Handler<Contract1>, IHandler>([null]);

        var contract = new Contract1();
        var unknownContract = new Contract2();
        var knownContract = new Contract1("second");
        var package = Factory<Package>().Generate() with { Contracts = [contract] };
        handler
            .Initialize(Arg.Any<Contract1>(), Arg.Any<string>())
            .Returns(info =>
                [
                    new ContractInitializeResult(info.Arg<Contract1>() with { Handler = handler }, [unknownContract]),
                ]
            );
        handler2
            .Initialize(Arg.Any<Contract1>(), Arg.Any<string>())
            .Returns(info =>
                [
                    new ContractInitializeResult(info.Arg<Contract1>() with { Handler = handler2 }, [knownContract])
                ]
            );

        var plan = Service.Create(package);

        Assert.NotNull(plan);
        Assert.Equal(3, plan.Contracts.Count());
        Assert.NotNull(plan.GetContract(package));
        Assert.NotNull(plan.GetContract(contract));
        Assert.NotNull(plan.GetContract(knownContract));
    }

    [Fact]
    public void Create_ContractWithSubstitute_CreatesDependentContract()
    {
        InstallPackage("docker");
        var container = Factory<Container>().Generate() with
        {
            Env = new Dictionary<string, string> { { "key", "{{Parameter:Test:Value}}" } }
        };
        var package = Factory<Package>().Generate() with { Contracts = [container] };

        var plan = Service.Create(package);

        var parameter = plan.GetContract(new ContractId<Parameter>("Test"));
        Assert.NotNull(parameter.Value);
    }

    [Fact]
    public void Create_ContractAddsSubstituteLater_CreatesDependentContract()
    {
        InstallPackage("docker");
        var container = Factory<Container>().Generate();
        var package = Factory<Package>().Generate() with
        {
            Contracts =
            [
                container,
                new Selector("")
                {
                    Options =
                    [
                        new SelectorOption(
                            "one",
                            [
                                container with
                                {
                                    Env = new Dictionary<string, string> { { "key", "{{Parameter:Test:Value}}" } }
                                }
                            ]
                        )
                    ]
                }
            ]
        };

        var plan = Service.Create(package);
        
        var parameter = plan.GetContract(new ContractId<Parameter>("Test"));
        Assert.NotNull(parameter.Value);
    }
}