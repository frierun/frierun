using Docker.DotNet;
using Docker.DotNet.Models;
using Frierun.Server;
using Frierun.Server.Data;
using NSubstitute;

namespace Frierun.Tests.Installers;

public class MysqlInstallerTests : BaseTests
{
    private readonly Application _providerApplication;
    
    public MysqlInstallerTests()
    {
        TryInstallPackage("docker");
        _providerApplication = TryInstallPackage("mysql")
            ?? throw new Exception("Mysql application not installed");
    }

    [Fact]
    public void Install_PackageWithContract_CreatesDatabase()
    {
        var package = Factory<Package>().Generate() with { Contracts = [new Mysql()] };

        var application = TryInstallPackage(package);

        Assert.NotNull(application);
        var database = application.Resources.OfType<MysqlDatabase>().First();
        Assert.Equal(package.Name, database.User);
        Assert.Equal(package.Name, database.Database);
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
        var package = Factory<Package>().Generate() with
        {
            Contracts =
            [
                new Mysql("first"),
                new Mysql("second"),
            ]
        };

        var application = TryInstallPackage(package);

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
        var package = Factory<Package>().Generate() with
        {
            Contracts =
            [
                new Mysql("first"),
                new Mysql("second"),
            ]
        };
        var application = TryInstallPackage(package);
        Assert.NotNull(application);

        Resolve<UninstallService>().Handle(application);

        DockerClient.Networks.Received(1).DisconnectNetworkAsync(
            application.Name,
            Arg.Any<NetworkDisconnectParameters>()
        );
    }
}