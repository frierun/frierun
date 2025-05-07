using Docker.DotNet.Models;
using Frierun.Server;
using Frierun.Server.Data;
using NSubstitute;

namespace Frierun.Tests.Installers;

public class PostgresqlInstallerTests : BaseTests
{
    [Fact]
    public void Install_PackageWithContract_CreatesDatabase()
    {
        var providerApplication = InstallPackage("postgresql") 
                                  ?? throw new Exception("Postgresql application not installed");
        
        var package = Factory<Package>().Generate() with
        {
            Contracts =
            [
                new Postgresql(),
            ]
        };

        var application = InstallPackage(package);

        Assert.NotNull(application);
        var database = application.Resources.OfType<PostgresqlDatabase>().First();
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
        InstallPackage("postgresql");
        
        var package = Factory<Package>().Generate() with
        {
            Contracts =
            [
                new Postgresql("first"),
                new Postgresql("second"),
            ]
        };

        var application = InstallPackage(package);

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
        InstallPackage("postgresql");
        var package = Factory<Package>().Generate() with
        {
            Contracts =
            [
                new Postgresql("first"),
                new Postgresql("second"),
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