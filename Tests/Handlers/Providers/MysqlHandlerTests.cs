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

        var database = State.GetContract(application, mysql.Id);
        Assert.True(database.Installed);
        Assert.StartsWith(package.Name, database.Username);
        Assert.StartsWith(package.Name, database.Database);
        Assert.Contains(_providerApplication.Name, application.RequiredApplications);
        Assert.Equal(application.Name, database.NetworkName);
        DockerClient.Networks.Received(1).ConnectNetworkAsync(
            database.NetworkName,
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

        Assert.True(State.GetContract(application, mysql1.Id).Installed);
        Assert.True(State.GetContract(application, mysql2.Id).Installed);
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
    public void Initialize_PrefixIsRoot_UserIsNotRoot()
    {
        var mysql = Contract<Mysql>().Generate();
        var package = Factory<Package>().Generate() with { Prefix = "root", Contracts = [mysql] };

        var application = InstallPackage(package);

        Assert.NotEqual(package.Name, State.GetContract(application, mysql.Id).Username);
    }

    [Fact]
    public void Initialize_PrefixIsMysql_DatabaseIsNotMysql()
    {
        UninstallApplication(_providerApplication);
        InstallPackage("mariadb");
        var mysql = Contract<Mysql>().Generate();
        var package = Factory<Package>().Generate() with { Prefix = "mysql", Contracts = [mysql] };

        var application = InstallPackage(package);

        Assert.NotEqual(package.Name, State.GetContract(application, mysql.Id).Database);
    }
}