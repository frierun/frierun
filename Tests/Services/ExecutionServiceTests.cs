using Frierun.Server;
using Frierun.Server.Data;
using Frierun.Server.Installers;
using NSubstitute;
using Substitute = NSubstitute.Substitute;

namespace Frierun.Tests.Services;

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
        var contract = Substitute.For<Contract>("", null, null, null);
        var package = Factory<Package>().Generate() with { Contracts = [contract] };
        var service = Resolve<ExecutionService>();

        Assert.Throws<InstallerNotFoundException>(() => service.Create(package));
    }

    [Fact]
    public void Create_InstallerWithoutOptions_ThrowsException()
    {
        var installer = Mock<IInstaller<Contract1>, IInstaller>();
        
        var contract = new Contract1();
        var package = Factory<Package>().Generate() with { Contracts = [contract] };
        var service = Resolve<ExecutionService>();
        installer
            .Initialize(Arg.Any<Contract>(), Arg.Any<string>())
            .Returns([]);

        Assert.Throws<InstallerNotFoundException>(() => service.Create(package));
    }

    [Fact]
    public void Create_InstallerReturnsUnknownContract_ThrowsException()
    {
        var installer = Mock<IInstaller<Contract1>, IInstaller>();
        
        var contract = new Contract1();
        var unknownContract = new Contract2();
        var package = Factory<Package>().Generate() with { Contracts = [contract] };
        var service = Resolve<ExecutionService>();
        installer
            .Initialize(Arg.Any<Contract>(), Arg.Any<string>())
            .Returns([new InstallerInitializeResult(contract, null, [unknownContract])]);

        Assert.Throws<InstallerNotFoundException>(() => service.Create(package));
    }
    
    [Fact]
    public void Create_CorrectInstaller_ExecutesInitialize()
    {
        var installer = Mock<IInstaller<Contract1>, IInstaller>();
        
        var contract = new Contract1();
        var package = Factory<Package>().Generate() with { Contracts = [contract] };
        var service = Resolve<ExecutionService>();
        installer
            .Initialize(Arg.Any<Contract>(), Arg.Any<string>())
            .Returns([new InstallerInitializeResult(contract)]);

        var plan = service.Create(package);

        Assert.NotNull(plan);
        Assert.Equal(2, plan.Contracts.Count);
        Assert.Contains(package.Id, plan.Contracts);
        Assert.Contains(contract.Id, plan.Contracts);
        installer.Received(1).Initialize(Arg.Any<Contract>(), Arg.Any<string>());
    }
    
    [Fact]
    public void Create_RecursiveInstaller_InitializesPlan()
    {
        var installer = Mock<IInstaller<Contract1>, IInstaller>();
        
        var contract = new Contract1();
        var package = Factory<Package>().Generate() with { Contracts = [contract] };
        var service = Resolve<ExecutionService>();
        installer
            .Initialize(Arg.Any<Contract>(), Arg.Any<string>())
            .Returns([new InstallerInitializeResult(contract, null, [contract])]);

        var plan = service.Create(package);

        Assert.NotNull(plan);
        Assert.Equal(2, plan.Contracts.Count);
        Assert.Contains(package.Id, plan.Contracts);
        Assert.Contains(contract.Id, plan.Contracts);
    }

    [Fact]
    public void Create_InstallerWithTwoBranches_InitializesPlan()
    {
        var installer = Mock<IInstaller<Contract1>, IInstaller>();
        
        var contract = new Contract1();
        var unknownContract = new Contract2();
        var knownContract = new Contract1("second");
        var package = Factory<Package>().Generate() with { Contracts = [contract] };
        var service = Resolve<ExecutionService>();
        installer
            .Initialize(Arg.Any<Contract>(), Arg.Any<string>())
            .Returns(info => [
                new InstallerInitializeResult(info.Arg<Contract>(), null, [unknownContract]),
                new InstallerInitializeResult(info.Arg<Contract>(), null, [knownContract])
            ]);

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
        var installer = Mock<IInstaller<Contract1>, IInstaller>();
        var installer2 = Mock<IInstaller<Contract1>, IInstaller>();
        
        var contract = new Contract1();
        var unknownContract = new Contract2();
        var knownContract = new Contract1("second");
        var package = Factory<Package>().Generate() with { Contracts = [contract] };
        var service = Resolve<ExecutionService>();
        installer
            .Initialize(Arg.Any<Contract>(), Arg.Any<string>())
            .Returns(info => [
                new InstallerInitializeResult(info.Arg<Contract>(), null, [unknownContract]),
            ]);
        installer2
            .Initialize(Arg.Any<Contract>(), Arg.Any<string>())
            .Returns(info => [
                new InstallerInitializeResult(info.Arg<Contract>(), null, [knownContract])
            ]);

        var plan = service.Create(package);

        Assert.NotNull(plan);
        Assert.Equal(3, plan.Contracts.Count);
        Assert.Contains(package.Id, plan.Contracts);
        Assert.Contains(contract.Id, plan.Contracts);
        Assert.Contains(knownContract.Id, plan.Contracts);
    }    
}