using Frierun.Server;
using Frierun.Server.Data;
using Microsoft.Extensions.DependencyInjection;

namespace Tests.Integration.Installers;

public class PostgresqlInstallerTests : TestWithDocker
{
    [Fact]
    public async Task Install_PostgresqlContract_CredentialsAreCorrect()
    {
        var dbPackage = Services.GetRequiredService<PackageRegistry>().Find("postgresql");
        Assert.NotNull(dbPackage);

        var dbApplication = InstallPackage(dbPackage);

        // wait for database to start
        Thread.Sleep(15000);

        var package = dbPackage with
        {
            Name = "db-client",
            Contracts = dbPackage.Contracts.Append(new Postgresql())
        };
        var application = InstallPackage(package);

        var container = application.Resources.OfType<DockerContainer>().First();
        var database = application.Resources.OfType<PostgresqlDatabase>().First();
        Assert.Equal("db-client", database.User);
        Assert.Equal("db-client", database.Database);
        Assert.Equal("postgresql", database.Host);
        Assert.NotEmpty(database.Password);

        // try to connect to the database from the client
        var sql =
            "CREATE TABLE Test (col int);INSERT INTO Test VALUES (123);UPDATE Test SET col = 2*col;SELECT * FROM Test;";
        var result = await DockerService.ExecInContainer(
            container.Name,
            [
                "psql",
                $"postgresql://{database.User}:{database.Password}@{database.Host}/{database.Database}",
                "-c", sql
            ]
        );

        Assert.Contains("246", result.stdout);

        // clean up
        UninstallApplication(application);
        UninstallApplication(dbApplication);
    }

    [Fact]
    public async Task Install_PostgresqlAdminContract_CredentialsAreCorrect()
    {
        var dbPackage = Services.GetRequiredService<PackageRegistry>().Find("postgresql");
        Assert.NotNull(dbPackage);

        var dbApplication = InstallPackage(dbPackage);

        // wait for database to start
        Thread.Sleep(15000);

        var package = dbPackage with
        {
            Name = "db-client",
            Contracts = dbPackage.Contracts.Append(new Postgresql(Admin: true))
        };
        var application = InstallPackage(package);

        var container = application.Resources.OfType<DockerContainer>().First();
        var database = application.Resources.OfType<PostgresqlDatabase>().First();
        Assert.Equal("postgres", database.User);
        Assert.Null(database.Database);
        Assert.Equal("postgresql", database.Host);
        Assert.NotEmpty(database.Password);

        // try to connect to the database from the client
        var sql =
            "SELECT CASE WHEN rolsuper='t' THEN 123+123 ELSE 123 END FROM pg_authid WHERE rolname=CURRENT_USER;";
        var (stdout, _) = await DockerService.ExecInContainer(
            container.Name,
            [
                "psql",
                $"postgresql://{database.User}:{database.Password}@{database.Host}/",
                "-c", sql
            ]
        );

        Assert.Contains("246", stdout);

        // clean up
        UninstallApplication(application);
        UninstallApplication(dbApplication);
    }
}