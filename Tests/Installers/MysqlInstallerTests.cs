using Frierun.Server;
using Frierun.Server.Data;

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
        Assert.Single(application.Resources.OfType<DockerAttachedNetwork>());
        Assert.Equal([providerApplication.Name], application.RequiredApplications);
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
        Assert.Single(application.Resources.OfType<DockerAttachedNetwork>());
    }
}