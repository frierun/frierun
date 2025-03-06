using Frierun.Server;
using Frierun.Server.Data;
using Microsoft.Extensions.DependencyInjection;

namespace Tests.Integration;

public class PostgresqlTests : BaseTests
{
    [Fact]
    public async Task Install_PostgresqlContract_CredentialsAreCorrect()
    {
        var state = Services.GetRequiredService<State>();
        Assert.Empty(state.Resources);
        
        var dbPackage = Services.GetRequiredService<PackageRegistry>().Find("postgresql");
        Assert.NotNull(dbPackage);

        var dbApplication = InstallPackage(dbPackage);
        Assert.NotNull(dbApplication);
        
        // wait for database to start
        Thread.Sleep(15000);

        var package = dbPackage with
        {
            Name = "db_client",
            Contracts = dbPackage.Contracts.Append(new Postgresql())
        };
        var application = InstallPackage(package);
        Assert.NotNull(application);

        var container = application.DependsOn.OfType<DockerContainer>().First();
        var database = application.DependsOn.OfType<PostgresqlDatabase>().First();
        Assert.Equal("db_client", database.User);
        Assert.Equal("db_client", database.Database);
        Assert.Equal("postgresql", database.Host);
        Assert.NotEmpty(database.Password);
        
        // try to connect to the database from the client
        var dockerService = Services.GetRequiredService<DockerService>();
        var sql = "CREATE TABLE Test (col int);INSERT INTO Test VALUES (123);UPDATE Test SET col = 2*col;SELECT * FROM Test;";
        var result = await dockerService.ExecInContainer(
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
        
        Assert.Empty(state.Resources);
    }    
}