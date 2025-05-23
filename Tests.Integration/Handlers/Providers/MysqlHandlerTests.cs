using Frierun.Server;
using Frierun.Server.Data;
using Microsoft.Extensions.DependencyInjection;

namespace Tests.Integration.Handlers;

public class MysqlHandlerTests : TestWithDocker
{
    [Theory]
    [InlineData("mysql")]
    [InlineData("mariadb")]
    public async Task Install_MysqlContract_CredentialsAreCorrect(string packageName)
    {
        var dbPackage = Services.GetRequiredService<PackageRegistry>().Find(packageName);
        Assert.NotNull(dbPackage);

        var dbApplication = InstallPackage(dbPackage);

        // wait for database to start
        Thread.Sleep(15000);

        var package = dbPackage with
        {
            Name = "db-client",
            Contracts = dbPackage.Contracts.Append(new Mysql())
        };
        var application = InstallPackage(package);

        var container = application.Contracts.OfType<Container>().Single();
        Assert.True(container.Installed);
        var database = application.Contracts.OfType<Mysql>().Single();
        Assert.True(database.Installed);
        Assert.Equal("db-client", database.Username);
        Assert.Equal("db-client", database.Database);
        Assert.Equal(packageName, database.Host);
        Assert.NotEmpty(database.Password);
        Assert.NotNull(database.Database);

        // try to connect to the database from the client
        var query =
            "CREATE TABLE Test (ID int);INSERT INTO Test VALUES (123);UPDATE Test SET ID = 2*ID;SELECT * FROM Test;";
        var result = await DockerService.ExecInContainer(
            container.ContainerName,
            [
                "mysql",
                "-u", database.Username,
                $"-p{database.Password}",
                "-h", database.Host,
                "-e", query,
                database.Database
            ]
        );

        Assert.Contains("246", result.stdout);

        // clean up
        UninstallApplication(application);
        UninstallApplication(dbApplication);
    }

    [Theory]
    [InlineData("mysql")]
    [InlineData("mariadb")]
    public async Task Install_MysqlAdminContract_CredentialsAreCorrect(string packageName)
    {
        var dbPackage = Services.GetRequiredService<PackageRegistry>().Find(packageName);
        Assert.NotNull(dbPackage);

        var dbApplication = InstallPackage(dbPackage);

        // wait for database to start
        Thread.Sleep(15000);

        var package = dbPackage with
        {
            Name = "db-client",
            Contracts = dbPackage.Contracts.Append(new Mysql(Admin: true))
        };
        var application = InstallPackage(package);

        var container = application.Contracts.OfType<Container>().Single();
        Assert.True(container.Installed);
        var database = application.Contracts.OfType<Mysql>().Single();
        Assert.True(database.Installed);
        Assert.Equal("root", database.Username);
        Assert.Null(database.Database);
        Assert.Equal(packageName, database.Host);
        Assert.NotEmpty(database.Password);

        // try to connect to the database from the client
        var query =
            "SHOW GRANTS";
        var result = await DockerService.ExecInContainer(
            container.ContainerName,
            [
                "mysql",
                "-u", database.Username,
                $"-p{database.Password}",
                "-h", database.Host,
                "-e", query
            ]
        );

        Assert.Contains(" ON *.* TO ", result.stdout);

        // clean up
        UninstallApplication(application);
        UninstallApplication(dbApplication);
    }
}