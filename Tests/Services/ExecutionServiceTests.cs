using Frierun.Server;
using Frierun.Server.Data;
using Frierun.Server.Handlers;
using Frierun.Tests.Factories;
using NSubstitute;
using Substitute = NSubstitute.Substitute;

namespace Frierun.Tests;

public class ExecutionServiceTests : BaseTests
{
    public record Contract1 : Contract
    {
        public override Contract Merge(Contract other)
        {
            return this;
        }
    }

    public record Contract2 : Contract
    {
        public override Contract Merge(Contract other)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Mocks handler for specified contract type.
    /// </summary>
    private Handler<TContract> MockHandler<TContract>()
        where TContract : Contract
    {
        return Mock<Handler<TContract>, IHandler>([Substitute.For<State>(), null]);
    }
    
    private ExecutionService Service => Resolve<ExecutionService>();

    [Fact]
    public void Create_EmptyPackage_ReturnsPlan()
    {
        var package = Factory<Package>().Generate();

        var plan = Service.Create(package.CreateApplication());

        Assert.NotNull(plan);
        Assert.Single(plan.Contracts);
        Assert.IsType<Application>(plan.Contracts.Values.First());
    }

    [Fact]
    public void Create_WithoutHandler_ThrowsException()
    {
        var contract = new ContractEntry<Contract>("", Substitute.For<Contract>());
        var package = Factory<Package>().Generate() with { Contracts = [contract] };

        Assert.Throws<HandlerNotFoundException>(() => Service.Create(package.CreateApplication()));
    }

    [Fact]
    public void Create_HandlerWithoutOptions_ThrowsException()
    {
        var handler = MockHandler<Contract1>();

        var contract = new ContractEntry<Contract1>("", new Contract1());
        var package = Factory<Package>().Generate() with { Contracts = [contract] };
        handler
            .Initialize(Arg.Any<Contract1>(), Arg.Any<ApplicationContext>())
            .Returns([]);

        Assert.Throws<HandlerNotFoundException>(() => Service.Create(package.CreateApplication()));
    }

    [Fact]
    public void Create_HandlerReturnsUnknownContract_ThrowsException()
    {
        var handler = MockHandler<Contract1>();

        var contract = new ContractEntry<Contract1>("", new Contract1());
        var unknownContract = new ContractEntry<Contract2>("", new Contract2());
        var package = Factory<Package>().Generate() with { Contracts = [contract] };
        handler
            .Initialize(Arg.Any<Contract1>(), Arg.Any<ApplicationContext>())
            .Returns([[contract.With(c => c with { Handler = handler }), unknownContract]]);

        Assert.Throws<HandlerNotFoundException>(() => Service.Create(package.CreateApplication()));
    }

    [Fact]
    public void Create_CorrectHandler_ExecutesInitialize()
    {
        var handler = MockHandler<Contract1>();

        var contract = new ContractEntry<Contract1>("", new Contract1());
        var package = Factory<Package>().Generate() with { Contracts = [contract] };
        handler
            .Initialize(Arg.Any<Contract1>(), Arg.Any<ApplicationContext>())
            .Returns([[contract.With(c => c with { Handler = handler })]]);

        var plan = Service.Create(package.CreateApplication());

        Assert.NotNull(plan);
        Assert.Equal(2, plan.Contracts.Count);
        Assert.NotNull(plan.GetContract(contract.Ref));
        // ReSharper disable once IteratorMethodResultIsIgnored
        handler.Received(1).Initialize(Arg.Any<Contract1>(), Arg.Any<ApplicationContext>());
    }

    [Fact]
    public void Create_RecursiveHandler_ThrowsException()
    {
        var handler = MockHandler<Contract1>();

        var contract1 = new ContractEntry<Contract1>("contract1", new Contract1 { Handler = handler });
        var contract2 = new ContractEntry<Contract1>("contract2", new Contract1 { Handler = handler });
        var package = Factory<Package>().Generate() with { Contracts = [contract1] };
        handler
            .Initialize(Arg.Any<Contract1>(), Arg.Any<ApplicationContext>())
            .Returns([[contract1, contract2]]);

        Assert.Throws<Exception>(() => Service.Create(package.CreateApplication()));
    }

    [Fact]
    public void Create_HandlerWithTwoBranches_InitializesPlan()
    {
        var handler = MockHandler<Contract1>();

        var contract = new ContractEntry<Contract1>("", new Contract1());
        var unknownContract = new ContractEntry<Contract2>("", new Contract2());
        var knownContract = new ContractEntry<Contract1>("second", new Contract1());
        var package = Factory<Package>().Generate() with { Contracts = [contract] };
        handler
            .Initialize(contract.Contract, Arg.Any<ApplicationContext>())
            .Returns(info =>
                [
                    new ContractList
                    {
                        [info.Arg<ApplicationContext>().Name] = info.Arg<Contract1>() with { Handler = handler },
                        [unknownContract.Ref] = unknownContract.Contract
                    },
                    new ContractList
                    {
                        [info.Arg<ApplicationContext>().Name] = info.Arg<Contract1>() with { Handler = handler },
                        [knownContract.Ref] = knownContract.Contract
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
                        [unknownContract.Ref] = unknownContract.Contract
                    },
                    new ContractList
                    {
                        [info.Arg<ApplicationContext>().Name] = info.Arg<Contract1>() with { Handler = handler }
                    }
                ]
            );

        var plan = Service.Create(package.CreateApplication());

        Assert.NotNull(plan);
        Assert.Equal(3, plan.Contracts.Count);
        Assert.NotNull(plan.GetContract(contract.Ref));
        Assert.NotNull(plan.GetContract(knownContract.Ref));
    }

    [Fact]
    public void Create_TwoHandlers_InitializesPlan()
    {
        var handler = MockHandler<Contract1>();
        var handler2 = MockHandler<Contract1>();

        var contract = new ContractEntry<Contract1>("", new Contract1());
        var unknownContract = new ContractEntry<Contract2>("", new Contract2());
        var knownContract = new ContractEntry<Contract1>("second", new Contract1());
        var package = Factory<Package>().Generate() with { Contracts = [contract] };
        handler
            .Initialize(Arg.Any<Contract1>(), Arg.Any<ApplicationContext>())
            .Returns(info =>
                [
                    new ContractList
                    {
                        [info.Arg<ApplicationContext>().Name] = info.Arg<Contract1>() with { Handler = handler },
                        [unknownContract.Ref] = unknownContract.Contract
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
                        [knownContract.Ref] = knownContract.Contract
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

        var plan = Service.Create(package.CreateApplication());

        Assert.NotNull(plan);
        Assert.Equal(3, plan.Contracts.Count);
        Assert.NotNull(plan.GetContract(contract.Ref));
        Assert.NotNull(plan.GetContract(knownContract.Ref));
    }

    [Fact]
    public void Create_ContractWithSubstitute_CreatesDependentContract()
    {
        InstallPackage("docker");
        var container = Contract<Container>()
            .Set(p => p.Env, new Dictionary<string, Argument<string>> { { "key", "{{Parameter:Test:Value}}" } })
            .Generate();
        var package = Factory<Package>().Generate() with { Contracts = [container] };

        var plan = Service.Create(package.CreateApplication());

        var parameter = plan.GetContract(new ContractRef<Parameter>("Test"));
        Assert.NotNull(parameter.Value);
    }

    [Fact]
    public void Create_ContractAddsSubstituteLater_CreatesDependentContract()
    {
        InstallPackage("docker");
        var container = Contract<Container>().Generate();
        var selector = Contract<Selector>()
            .Set(
                p => p.Options,
                [
                    new SelectorOption(
                        "one",
                        [
                            container.With(c => c with
                                {
                                    Env = new Dictionary<string, Argument<string>>
                                        { { "key", "{{Parameter:Test:Value}}" } }
                                }
                            )
                        ]
                    )
                ]
            )
            .Generate();
        var package = Factory<Package>().Generate() with { Contracts = [container, selector] };

        var plan = Service.Create(package.CreateApplication());

        var parameter = plan.GetContract(new ContractRef<Parameter>("Test"));
        Assert.NotNull(parameter.Value);
    }

    [Fact]
    public void Create_ContractWithoutRestrictedApplicationHandler_ReturnsBothVariants()
    {
        InstallPackage("docker");
        InstallPackage("docker");
        var container = Contract<Container>().Generate();
        var package = Factory<Package>().Generate() with { Contracts = [container] };

        var plan = Service.Create(package.CreateApplication());

        Assert.Single(plan.Alternatives, alternative => alternative.ContractRef == container.Ref);
    }

    [Fact]
    public void Create_ContractWithRestrictedApplicationHandler_ReturnsSingleVariant()
    {
        var docker1 = InstallPackage("docker");
        var docker2 = InstallPackage("docker");
        var container = Contract<Container>().Generate();
        var package1 = Factory<Package>().Generate() with
        {
            Contracts = [container.With(c => c with { HandlerApplication = docker1.Name })]
        };
        var package2 = Factory<Package>().Generate() with
        {
            Contracts = [container.With(c => c with { HandlerApplication = docker2.Name })]
        };

        var plan1 = Service.Create(package1.CreateApplication());
        var plan2 = Service.Create(package2.CreateApplication());

        Assert.DoesNotContain(plan1.Alternatives, alternative => alternative.ContractRef == container.Ref);
        Assert.DoesNotContain(plan2.Alternatives, alternative => alternative.ContractRef == container.Ref);
    }
}