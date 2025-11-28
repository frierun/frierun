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
        var postgresql = Contract<Postgresql>().Generate();
        var package = Factory<Package>().Generate() with { Contracts = [postgresql] };

        var application = InstallPackage(package);

        var database = State.GetContract(application, postgresql.Ref);
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
        var postgresql1 = Contract<Postgresql>().Generate();
        var postgresql2 = Contract<Postgresql>().Generate();
        var package = Factory<Package>().Generate() with { Contracts = [postgresql1, postgresql2] };

        var application = InstallPackage(package);

        Assert.True(State.GetContract(application, postgresql1.Ref).Installed);
        Assert.True(State.GetContract(application, postgresql2.Ref).Installed);
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
                Contract<Postgresql>().Generate(),
                Contract<Postgresql>().Generate()
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
        var postgresql = Factory<Postgresql>().Generate();
        var package = Factory<Package>().Generate() with { Contracts = new ContractList { [""] = postgresql } };

        var application = InstallPackage(package);

        Assert.Equal(package.Name, application.Name);
        Assert.Equal(package.Name, State.GetContract<Postgresql>(application).Username);
        Assert.Equal(package.Name, State.GetContract<Postgresql>(application).Database);
    }

    [Fact]
    public void Initialize_EmptyIdWithPrefixPostgres_UserIsNotPostgres()
    {
        var postgresql = Factory<Postgresql>().Generate();
        var package = Factory<Package>().Generate() with
        {
            Name = "postgres", Contracts = new ContractList { [""] = postgresql }
        };

        var application = InstallPackage(package);

        Assert.Equal(package.Name, application.Name);
        Assert.NotEqual(package.Name, State.GetContract<Postgresql>(application).Username);
        Assert.Equal(package.Name, State.GetContract<Postgresql>(application).Database);
    }
}