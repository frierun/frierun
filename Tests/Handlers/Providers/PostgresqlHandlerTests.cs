using Docker.DotNet.Models;
using Frierun.Server;
using Frierun.Server.Data;
using NSubstitute;

namespace Frierun.Tests.Handlers;

public class PostgresqlHandlerTests : BaseTests
{
    private readonly Application _providerApplication;

    public PostgresqlHandlerTests()
    {
        InstallPackage("docker");
        _providerApplication = InstallPackage("postgresql");
    }

    [Fact]
    public void Install_PackageWithContract_CreatesDatabase()
    {
        var package = Factory<Package>().Generate() with
        {
            Contracts = new ContractList { Contract<Postgresql>().Generate() }
        };

        var application = InstallPackage(package);

        var database = application.GetContracts<Postgresql>().Single();
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
        var package = Factory<Package>().Generate() with
        {
            Contracts = new ContractList
            {
                Contract<Postgresql>().Generate(),
                Contract<Postgresql>().Generate()
            }
        };

        var application = InstallPackage(package);

        Assert.Equal(2, application.GetContracts<Postgresql>().Count());
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
            Contracts = new ContractList
            {
                Contract<Postgresql>().Generate(),
                Contract<Postgresql>().Generate()
            }
        };
        var application = InstallPackage(package);

        Resolve<UninstallService>().Handle(application);

        DockerClient.Networks.Received(1).DisconnectNetworkAsync(
            application.Name,
            Arg.Any<NetworkDisconnectParameters>()
        );
    }

    [Fact]
    public void Initialize_PrefixIsPostgres_UserIsNotPostgres()
    {
        var package = Factory<Package>().Generate() with
        {
            Prefix = "postgres",
            Contracts = new ContractList { Contract<Postgresql>().Generate() }
        };

        var application = InstallPackage(package);

        var database = application.GetContracts<Postgresql>().Single();
        Assert.NotEqual(package.Name, database.Username);
    }
}