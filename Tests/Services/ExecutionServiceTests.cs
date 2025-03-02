using Frierun.Server.Data;
using Frierun.Server.Installers;
using Frierun.Server.Services;
using NSubstitute;

namespace Frierun.Tests.Services;

public class ExecutionServiceTests : BaseTests
{
    [Fact]
    public void Create_EmptyPackage_ReturnsPlan()
    {
        var package = Factory<Package>().Generate();
        var service = Resolve<ExecutionService>();

        var plan = service.Create(package);
        
        Assert.NotNull(plan);
        Assert.Contains(package.Id, plan.Contracts);
    }
    
    [Fact]
    public void Create_MysqlContract_ThrowsException()
    {
        var package = Factory<Package>().Generate() with { Contracts = [new Mysql()] };
        var service = Resolve<ExecutionService>();

        Assert.Throws<Exception>(() => service.Create(package));
    }
    
    [Fact]
    public void Create_MysqlContractWithInstaller_ExecutesInitialize()
    {
        var installer = Mock<IInstaller<Mysql>, IInstaller>();
        var mysqlContract = new Mysql();
        var package = Factory<Package>().Generate() with { Contracts = [mysqlContract] };
        var service = Resolve<ExecutionService>();
        installer
            .Initialize(Arg.Any<Contract>(), Arg.Any<string>(), Arg.Any<State>())
            .Returns(new InstallerInitializeResult(mysqlContract));

        var plan = service.Create(package);

        Assert.NotNull(plan);
        Assert.Contains(package.Id, plan.Contracts);
        Assert.Contains(mysqlContract.Id, plan.Contracts);
        installer.Received(1).Initialize(Arg.Any<Contract>(), Arg.Any<string>(), Arg.Any<State>());
    }
    
}