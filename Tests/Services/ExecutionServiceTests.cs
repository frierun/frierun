using Frierun.Server;
using Frierun.Server.Data;
using Frierun.Server.Installers;
using NSubstitute;
using Substitute = NSubstitute.Substitute;

namespace Frierun.Tests;

public class ExecutionServiceTests : BaseTests
{
    public record Contract1(string? Name = null) : Contract(Name ?? "");

    public record Contract2(string? Name = null) : Contract(Name ?? "");

    [Fact]
    public void Create_EmptyPackage_ReturnsPlan()
    {
        var package = Factory<Package>().Generate();
        var service = Resolve<ExecutionService>();

        var plan = service.Create(package);

        Assert.NotNull(plan);
        Assert.Single(plan.Contracts);
        Assert.Contains(package.Id, plan.Contracts);
    }

    [Fact]
    public void Create_WithoutInstaller_ThrowsException()
    {
        var contract = Substitute.For<Contract>("", false, null, null, null);
        var package = Factory<Package>().Generate() with { Contracts = [contract] };
        var service = Resolve<ExecutionService>();

        Assert.Throws<InstallerNotFoundException>(() => service.Create(package));
    }

    [Fact]
    public void Create_HandlerWithoutOptions_ThrowsException()
    {
        var handler = Mock<IHandler<Contract1>, IHandler>();

        var contract = new Contract1();
        var package = Factory<Package>().Generate() with { Contracts = [contract] };
        var service = Resolve<ExecutionService>();
        handler
            .Initialize(Arg.Any<Contract>(), Arg.Any<string>())
            .Returns([]);

        Assert.Throws<InstallerNotFoundException>(() => service.Create(package));
    }

    [Fact]
    public void Create_HandlerReturnsUnknownContract_ThrowsException()
    {
        var handler = Mock<IHandler<Contract1>, IHandler>();

        var contract = new Contract1();
        var unknownContract = new Contract2();
        var package = Factory<Package>().Generate() with { Contracts = [contract] };
        var service = Resolve<ExecutionService>();
        handler
            .Initialize(Arg.Any<Contract>(), Arg.Any<string>())
            .Returns([new InstallerInitializeResult(contract, [unknownContract])]);

        Assert.Throws<InstallerNotFoundException>(() => service.Create(package));
    }

    [Fact]
    public void Create_CorrectHandler_ExecutesInitialize()
    {
        var handler = Mock<IHandler<Contract1>, IHandler>();

        var contract = new Contract1();
        var package = Factory<Package>().Generate() with { Contracts = [contract] };
        var service = Resolve<ExecutionService>();
        handler
            .Initialize(Arg.Any<Contract>(), Arg.Any<string>())
            .Returns([new InstallerInitializeResult(contract with { Handler = handler })]);

        var plan = service.Create(package);

        Assert.NotNull(plan);
        Assert.Equal(2, plan.Contracts.Count);
        Assert.Contains(package.Id, plan.Contracts);
        Assert.Contains(contract.Id, plan.Contracts);
        handler.Received(1).Initialize(Arg.Any<Contract>(), Arg.Any<string>());
    }

    [Fact]
    public void Create_RecursiveHandler_InitializesPlan()
    {
        var handler = Mock<IHandler<Contract1>, IHandler>();

        var contract = new Contract1();
        var package = Factory<Package>().Generate() with { Contracts = [contract] };
        var service = Resolve<ExecutionService>();
        handler
            .Initialize(Arg.Any<Contract>(), Arg.Any<string>())
            .Returns([new InstallerInitializeResult(contract with { Handler = handler }, [contract])]);

        var plan = service.Create(package);

        Assert.NotNull(plan);
        Assert.Equal(2, plan.Contracts.Count);
        Assert.Contains(package.Id, plan.Contracts);
        Assert.Contains(contract.Id, plan.Contracts);
    }

    [Fact]
    public void Create_HandlerWithTwoBranches_InitializesPlan()
    {
        var handler = Mock<IHandler<Contract1>, IHandler>();

        var contract = new Contract1();
        var unknownContract = new Contract2();
        var knownContract = new Contract1("second");
        var package = Factory<Package>().Generate() with { Contracts = [contract] };
        var service = Resolve<ExecutionService>();
        handler
            .Initialize(Arg.Any<Contract>(), Arg.Any<string>())
            .Returns(
                info =>
                [
                    new InstallerInitializeResult(info.Arg<Contract>() with { Handler = handler }, [unknownContract]),
                    new InstallerInitializeResult(info.Arg<Contract>() with { Handler = handler }, [knownContract])
                ]
            );

        var plan = service.Create(package);

        Assert.NotNull(plan);
        Assert.Equal(3, plan.Contracts.Count);
        Assert.Contains(package.Id, plan.Contracts);
        Assert.Contains(contract.Id, plan.Contracts);
        Assert.Contains(knownContract.Id, plan.Contracts);
    }

    [Fact]
    public void Create_TwoInstallers_InitializesPlan()
    {
        var handler = Mock<IHandler<Contract1>, IHandler>();
        var handler2 = Mock<IHandler<Contract1>, IHandler>();

        var contract = new Contract1();
        var unknownContract = new Contract2();
        var knownContract = new Contract1("second");
        var package = Factory<Package>().Generate() with { Contracts = [contract] };
        var service = Resolve<ExecutionService>();
        handler
            .Initialize(Arg.Any<Contract>(), Arg.Any<string>())
            .Returns(
                info =>
                [
                    new InstallerInitializeResult(info.Arg<Contract>() with { Handler = handler }, [unknownContract]),
                ]
            );
        handler2
            .Initialize(Arg.Any<Contract>(), Arg.Any<string>())
            .Returns(
                info =>
                [
                    new InstallerInitializeResult(info.Arg<Contract>() with { Handler = handler2 }, [knownContract])
                ]
            );

        var plan = service.Create(package);

        Assert.NotNull(plan);
        Assert.Equal(3, plan.Contracts.Count);
        Assert.Contains(package.Id, plan.Contracts);
        Assert.Contains(contract.Id, plan.Contracts);
        Assert.Contains(knownContract.Id, plan.Contracts);
    }
}