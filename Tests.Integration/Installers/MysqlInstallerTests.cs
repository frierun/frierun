using Frierun.Server;
using Frierun.Server.Data;
using Microsoft.Extensions.DependencyInjection;

namespace Tests.Integration.Installers;

public class MysqlInstallerTests : TestWithDocker
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

        var container = application.Resources.OfType<DockerContainer>().First();
        var database = application.Resources.OfType<MysqlDatabase>().First();
        Assert.Equal("db-client", database.User);
        Assert.Equal("db-client", database.Database);
        Assert.Equal(packageName, database.Host);
        Assert.NotEmpty(database.Password);
        Assert.NotNull(database.Database);

        // try to connect to the database from the client
        var dockerService = Services.GetRequiredService<DockerService>();
        var query =
            "CREATE TABLE Test (ID int);INSERT INTO Test VALUES (123);UPDATE Test SET ID = 2*ID;SELECT * FROM Test;";
        var result = await dockerService.ExecInContainer(
            container.Name,
            [
                "mysql",
                "-u", database.User,
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

        var container = application.Resources.OfType<DockerContainer>().First();
        var database = application.Resources.OfType<MysqlDatabase>().First();
        Assert.Equal("root", database.User);
        Assert.Null(database.Database);
        Assert.Equal(packageName, database.Host);
        Assert.NotEmpty(database.Password);

        // try to connect to the database from the client
        var dockerService = Services.GetRequiredService<DockerService>();
        var query =
            "SHOW GRANTS";
        var result = await dockerService.ExecInContainer(
            container.Name,
            [
                "mysql",
                "-u", database.User,
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