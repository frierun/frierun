using Docker.DotNet.Models;
using Frierun.Server;
using Frierun.Server.Data;
using NSubstitute;

namespace Frierun.Tests.Installers;

public class PostgresqlInstallerTests : BaseTests
{
    private readonly Application _providerApplication;
    
    public PostgresqlInstallerTests()
    {
        TryInstallPackage("docker");
        _providerApplication = TryInstallPackage("postgresql")
                               ?? throw new Exception("Postgresql application not installed");
        
    }

    [Fact]
    public void Install_PackageWithContract_CreatesDatabase()
    {
        var package = Factory<Package>().Generate() with
        {
            Contracts =
            [
                new Postgresql(),
            ]
        };

        var application = TryInstallPackage(package);

        Assert.NotNull(application);
        var database = application.Resources.OfType<PostgresqlDatabase>().First();
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
                new Postgresql("first"),
                new Postgresql("second"),
            ]
        };

        var application = TryInstallPackage(package);

        Assert.NotNull(application);
        Assert.Equal(2, application.Resources.OfType<PostgresqlDatabase>().Count());
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
                new Postgresql("first"),
                new Postgresql("second"),
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