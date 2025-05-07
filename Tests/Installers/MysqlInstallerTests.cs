using Docker.DotNet;
using Docker.DotNet.Models;
using Frierun.Server;
using Frierun.Server.Data;
using NSubstitute;

namespace Frierun.Tests.Installers;

public class MysqlInstallerTests : BaseTests
{
    [Fact]
    public void Install_PackageWithContract_CreatesDatabase()
    {
        var providerApplication = InstallPackage("mysql")
                                  ?? throw new Exception("Mysql application not installed");
        var package = Factory<Package>().Generate() with { Contracts = [new Mysql()] };

        var application = InstallPackage(package);

        Assert.NotNull(application);
        var database = application.Resources.OfType<MysqlDatabase>().First();
        Assert.Equal(package.Name, database.User);
        Assert.Equal(package.Name, database.Database);
        Assert.Equal([providerApplication.Name], application.RequiredApplications);
        Assert.Equal(application.Name, database.NetworkName);
        DockerClient.Networks.Received(1).ConnectNetworkAsync(
            database.NetworkName, 
            Arg.Any<NetworkConnectParameters>()
        );
    }

    [Fact]
    public void Install_PackageWithTwoContracts_OnlyOneNetworkAttached()
    {
        InstallPackage("mysql");

        var package = Factory<Package>().Generate() with
        {
            Contracts =
            [
                new Mysql("first"),
                new Mysql("second"),
            ]
        };

        var application = InstallPackage(package);

        Assert.NotNull(application);
        Assert.Equal(2, application.Resources.OfType<MysqlDatabase>().Count());
        DockerClient.Networks.Received(1).ConnectNetworkAsync(
            application.Name,
            Arg.Any<NetworkConnectParameters>()
        );
    }
    
    [Fact]
    public void Uninstall_PackageWithTwoContracts_OnlyOneNetworkDetached()
    {
        InstallPackage("mysql");
        var package = Factory<Package>().Generate() with
        {
            Contracts =
            [
                new Mysql("first"),
                new Mysql("second"),
            ]
        };
        var application = InstallPackage(package);
        Assert.NotNull(application);
        
        Resolve<UninstallService>().Handle(application);

        DockerClient.Networks.Received(1).DisconnectNetworkAsync(
            application.Name,
            Arg.Any<NetworkDisconnectParameters>()
        );
    }    
}