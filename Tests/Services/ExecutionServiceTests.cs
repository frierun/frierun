using Frierun.Server;
using Frierun.Server.Data;
using Frierun.Server.Handlers;
using Frierun.Tests.Factories;
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
        var contract = new ContractEntry<Contract>("", Substitute.For<Contract>(""));
        var package = Factory<Package>().Generate() with { Contracts = new ContractList { contract } };

        Assert.Throws<HandlerNotFoundException>(() => Service.Create(package));
    }

    [Fact]
    public void Create_HandlerWithoutOptions_ThrowsException()
    {
        var handler = Mock<Handler<Contract1>, IHandler>([null]);

        var contract = new ContractEntry<Contract1>("", new Contract1());
        var package = Factory<Package>().Generate() with { Contracts = new ContractList { contract } };
        handler
            .Initialize(Arg.Any<Contract1>(), Arg.Any<ApplicationContext>())
            .Returns([]);

        Assert.Throws<HandlerNotFoundException>(() => Service.Create(package));
    }

    [Fact]
    public void Create_HandlerReturnsUnknownContract_ThrowsException()
    {
        var handler = Mock<Handler<Contract1>, IHandler>([null]);

        var contract = new ContractEntry<Contract1>("", new Contract1());
        var unknownContract = new ContractEntry<Contract2>("", new Contract2());
        var package = Factory<Package>().Generate() with { Contracts = new ContractList { contract } };
        handler
            .Initialize(Arg.Any<Contract1>(), Arg.Any<ApplicationContext>())
            .Returns([new ContractList { contract.With(c => c with { Handler = handler }), unknownContract }]);

        Assert.Throws<HandlerNotFoundException>(() => Service.Create(package));
    }

    [Fact]
    public void Create_CorrectHandler_ExecutesInitialize()
    {
        var handler = Mock<Handler<Contract1>, IHandler>([null]);

        var contract = new ContractEntry<Contract1>("", new Contract1());
        var package = Factory<Package>().Generate() with { Contracts = new ContractList { contract } };
        handler
            .Initialize(Arg.Any<Contract1>(), Arg.Any<ApplicationContext>())
            .Returns([new ContractList { contract.With(c => c with { Handler = handler }) }]);

        var plan = Service.Create(package);

        Assert.NotNull(plan);
        Assert.Equal(2, plan.Contracts.Count());
        Assert.NotNull(plan.GetContract(package));
        Assert.NotNull(plan.GetContract(contract.Id));
        // ReSharper disable once IteratorMethodResultIsIgnored
        handler.Received(1).Initialize(Arg.Any<Contract1>(), Arg.Any<ApplicationContext>());
    }

    [Fact]
    public void Create_RecursiveHandler_ThrowsException()
    {
        var handler = Mock<Handler<Contract1>, IHandler>([null]);

        var contract1 = new ContractEntry<Contract1>("contract1", new Contract1("contract1") { Handler = handler });
        var contract2 = new ContractEntry<Contract1>("contract2", new Contract1("contract2") { Handler = handler });
        var package = Factory<Package>().Generate() with { Contracts = new ContractList { contract1 } };
        handler
            .Initialize(Arg.Any<Contract1>(), Arg.Any<ApplicationContext>())
            .Returns([new ContractList { contract1, contract2 }]);

        Assert.Throws<Exception>(() => Service.Create(package));
    }

    [Fact]
    public void Create_HandlerWithTwoBranches_InitializesPlan()
    {
        var handler = Mock<Handler<Contract1>, IHandler>([null]);

        var contract = new ContractEntry<Contract1>("", new Contract1());
        var unknownContract = new ContractEntry<Contract2>("", new Contract2());
        var knownContract = new ContractEntry<Contract1>("second", new Contract1("second"));
        var package = Factory<Package>().Generate() with { Contracts = new ContractList { contract } };
        handler
            .Initialize(contract.Contract, Arg.Any<ApplicationContext>())
            .Returns(info =>
                [
                    new ContractList
                    {
                        [info.Arg<ApplicationContext>().Name] = info.Arg<Contract1>() with { Handler = handler },
                        [unknownContract.Id] = unknownContract.Contract
                    },
                    new ContractList
                    {
                        [info.Arg<ApplicationContext>().Name] = info.Arg<Contract1>() with { Handler = handler },
                        [knownContract.Id] = knownContract.Contract
                    }
                ]
            );
        handler
            .Initialize(Arg.Is<Contract1>(arg => arg != contract.Contract), Arg.Any<ApplicationContext>())
            .Returns(info =>
                [
                    new ContractList
                    {
                        [info.Arg<ApplicationContext>().Name] = info.Arg<Contract1>() with { Handler = handler },
                        [unknownContract.Id] = unknownContract.Contract
                    },
                    new ContractList
                    {
                        [info.Arg<ApplicationContext>().Name] = info.Arg<Contract1>() with { Handler = handler }
                    }
                ]
            );

        var plan = Service.Create(package);

        Assert.NotNull(plan);
        Assert.Equal(3, plan.Contracts.Count());
        Assert.NotNull(plan.GetContract(package));
        Assert.NotNull(plan.GetContract(contract.Id));
        Assert.NotNull(plan.GetContract(knownContract.Id));
    }

    [Fact]
    public void Create_TwoHandlers_InitializesPlan()
    {
        var handler = Mock<Handler<Contract1>, IHandler>([null]);
        var handler2 = Mock<Handler<Contract1>, IHandler>([null]);

        var contract = new ContractEntry<Contract1>("", new Contract1());
        var unknownContract = new ContractEntry<Contract2>("", new Contract2());
        var knownContract = new ContractEntry<Contract1>("second", new Contract1("second"));
        var package = Factory<Package>().Generate() with { Contracts = new ContractList { contract } };
        handler
            .Initialize(Arg.Any<Contract1>(), Arg.Any<ApplicationContext>())
            .Returns(info =>
                [
                    new ContractList
                    {
                        [info.Arg<ApplicationContext>().Name] = info.Arg<Contract1>() with { Handler = handler },
                        [unknownContract.Id] = unknownContract.Contract
                    },
                ]
            );

        handler2
            .Initialize(contract.Contract, Arg.Any<ApplicationContext>())
            .Returns(info =>
                [
                    new ContractList
                    {
                        [info.Arg<ApplicationContext>().Name] = info.Arg<Contract1>() with { Handler = handler2 },
                        [knownContract.Id] = knownContract.Contract
                    }
                ]
            );

        handler2
            .Initialize(Arg.Is<Contract1>(arg => arg != contract.Contract), Arg.Any<ApplicationContext>())
            .Returns(info =>
                [
                    new ContractList
                    {
                        [info.Arg<ApplicationContext>().Name] = info.Arg<Contract1>() with { Handler = handler2 }
                    }
                ]
            );

        var plan = Service.Create(package);

        Assert.NotNull(plan);
        Assert.Equal(3, plan.Contracts.Count());
        Assert.NotNull(plan.GetContract(package));
        Assert.NotNull(plan.GetContract(contract.Contract));
        Assert.NotNull(plan.GetContract(knownContract.Contract));
    }

    [Fact]
    public void Create_ContractWithSubstitute_CreatesDependentContract()
    {
        InstallPackage("docker");
        var container = Contract<Container>().Generate().With(c => c with
            {
                Env = new Dictionary<string, Argument<string>> { { "key", "{{Parameter:Test:Value}}" } }
            }
        );
        var package = Factory<Package>().Generate() with { Contracts = new ContractList { container } };

        var plan = Service.Create(package);

        var parameter = plan.GetContract(new ContractId<Parameter>("Test"));
        Assert.NotNull(parameter.Value);
    }

    [Fact]
    public void Create_ContractAddsSubstituteLater_CreatesDependentContract()
    {
        InstallPackage("docker");
        var container = Contract<Container>().Generate();
        var selector = Contract<Selector>().Generate().With(s => s with
            {
                Options =
                [
                    new SelectorOption(
                        "one",
                        new ContractList
                        {
                            container.With(c => c with
                                {
                                    Env = new Dictionary<string, Argument<string>>
                                        { { "key", "{{Parameter:Test:Value}}" } }
                                }
                            )
                        }
                    )
                ]
            }
        );
        var package = Factory<Package>().Generate() with
        {
            Contracts = new ContractList { container, selector }
        };

        var plan = Service.Create(package);

        var parameter = plan.GetContract(new ContractId<Parameter>("Test"));
        Assert.NotNull(parameter.Value);
    }

    [Fact]
    public void Create_ContractWithoutRestrictedApplicationHandler_ReturnsBothVariants()
    {
        InstallPackage("docker");
        InstallPackage("docker");
        var container = Contract<Container>().Generate();
        var package = Factory<Package>().Generate() with { Contracts = new ContractList { container } };

        var plan = Service.Create(package);

        Assert.Single(plan.Alternatives, contract => contract.Id == container.Id);
    }

    [Fact]
    public void Create_ContractWithRestrictedApplicationHandler_ReturnsSingleVariant()
    {
        var docker1 = InstallPackage("docker");
        var docker2 = InstallPackage("docker");
        var container = Contract<Container>().Generate();

        var plan1 = Service.Create(
            Factory<Package>().Generate() with
            {
                Contracts = new ContractList { container.With(c => c with { HandlerApplication = docker1.Name }) }
            }
        );
        var plan2 = Service.Create(
            Factory<Package>().Generate() with
            {
                Contracts = new ContractList { container.With(c => c with { HandlerApplication = docker2.Name }) }
            }
        );

        Assert.DoesNotContain(plan1.Alternatives, contract => contract.Id == container.Id);
        Assert.DoesNotContain(plan2.Alternatives, contract => contract.Id == container.Id);
    }
}