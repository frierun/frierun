using Docker.DotNet.Models;
using Frierun.Server;
using Frierun.Server.Data;
using NSubstitute;

namespace Frierun.Tests.Handlers;

public class MysqlHandlerTests : BaseTests
{
    private readonly Application _providerApplication;

    public MysqlHandlerTests()
    {
        InstallPackage("docker");
        _providerApplication = InstallPackage("mysql");
    }

    [Fact]
    public void Install_PackageWithContract_CreatesDatabase()
    {
        var mysql = Contract<Mysql>().Generate();
        var package = Factory<Package>().Generate() with { Contracts = [mysql] };

        var application = InstallPackage(package);

        var database = State.GetContract(application, mysql.Ref);
        Assert.True(database.Installed);
        Assert.StartsWith(package.Name, database.Username);
        Assert.StartsWith(package.Name, database.Database);
        Assert.Contains(_providerApplication.Name, application.RequiredApplications);

        var network = State.GetContract(database.Network);
        Assert.Equal(application.Name, network.NetworkName);
        DockerClient.Networks.Received(1).ConnectNetworkAsync(
            network.NetworkName,
            Arg.Any<NetworkConnectParameters>()
        );
    }

    [Fact]
    public void Install_PackageWithTwoContracts_OnlyOneNetworkAttached()
    {
        var mysql1 = Contract<Mysql>().Generate();
        var mysql2 = Contract<Mysql>().Generate();
        var package = Factory<Package>().Generate() with { Contracts = [mysql1, mysql2] };

        var application = InstallPackage(package);

        Assert.True(State.GetContract(application, mysql1.Ref).Installed);
        Assert.True(State.GetContract(application, mysql2.Ref).Installed);
        DockerClient.Networks.Received(1).ConnectNetworkAsync(
            application.Name,
            Arg.Any<NetworkConnectParameters>()
        );
    }

    [Fact]
    public void Uninstall_PackageWithTwoContracts_OnlyOneNetworkDetached()
    {
        var package = Factory<Package>().Generate() with
        {
            Contracts =
            [
                Contract<Mysql>().Generate(),
                Contract<Mysql>().Generate()
            ]
        };
        var application = InstallPackage(package);

        Resolve<UninstallService>().Handle(application);

        DockerClient.Networks.Received(1).DisconnectNetworkAsync(
            application.Name,
            Arg.Any<NetworkDisconnectParameters>()
        );
    }

    [Fact]
    public void Initialize_EmptyId_UserAndDatabaseEqualPackageName()
    {
        var mysql = Factory<Mysql>().Generate();
        var package = Factory<Package>().Generate() with { Contracts = new ContractList { [""] = mysql } };

        var application = InstallPackage(package);

        Assert.Equal(package.Name, application.Name);        
        Assert.Equal(package.Name, State.GetContract<Mysql>(application).Username);
        Assert.Equal(package.Name, State.GetContract<Mysql>(application).Database);
    }

    [Fact]
    public void Initialize_EmptyIdWithPrefixRoot_UserIsNotRoot()
    {
        var mysql = Factory<Mysql>().Generate();
        var package = Factory<Package>().Generate() with
        {
            Name = "root", Contracts = new ContractList { [""] = mysql }
        };

        var application = InstallPackage(package);

        Assert.Equal(package.Name, application.Name);
        Assert.NotEqual(package.Name, State.GetContract<Mysql>(application).Username);
        Assert.Equal(package.Name, State.GetContract<Mysql>(application).Database);
    }

    [Fact]
    public void Initialize_EmptyIdWithPrefixMysql_DatabaseIsNotMysql()
    {
        UninstallApplication(_providerApplication);
        InstallPackage("mariadb");
        var mysql = Factory<Mysql>().Generate();
        var package = Factory<Package>().Generate() with
        {
            Name = "mysql", Contracts = new ContractList { [""] = mysql }
        };

        var application = InstallPackage(package);

        Assert.Equal(package.Name, application.Name);
        Assert.Equal(package.Name, State.GetContract<Mysql>(application).Username);
        Assert.NotEqual(package.Name, State.GetContract<Mysql>(application).Database);
    }
}