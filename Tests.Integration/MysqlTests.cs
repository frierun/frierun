using Frierun.Server;
using Frierun.Server.Data;
using Frierun.Server.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Tests.Integration;

public class MysqlTests : BasicTests
{
    [Fact]
    public async Task Install_MysqlContract_CredentialsAreCorrect()
    {
        var mysqlPackage = Services.GetRequiredService<PackageRegistry>().Find("mysql");
        Assert.NotNull(mysqlPackage);

        var mysqlApplication = InstallPackage(mysqlPackage);
        Assert.NotNull(mysqlApplication);
        
        // wait for mysql to start
        Thread.Sleep(15000);

        var package = mysqlPackage with
        {
            Name = "mysql_client",
            Contracts = mysqlPackage.Contracts.Append(new Mysql())
        };
        var application = InstallPackage(package);
        Assert.NotNull(application);

        var container = application.DependsOn.OfType<DockerContainer>().First();
        var mysqlDatabase = application.DependsOn.OfType<MysqlDatabase>().First();
        Assert.Equal("mysql_client", mysqlDatabase.User);
        Assert.Equal("mysql_client", mysqlDatabase.Database);
        Assert.Equal("mysql", mysqlDatabase.Host);
        Assert.NotEmpty(mysqlDatabase.Password);

        // try to connect to mysql from client
        var dockerService = Services.GetRequiredService<DockerService>();
        var sql = "CREATE TABLE Test (ID int);INSERT INTO Test VALUES (123);UPDATE Test SET ID = 2*ID;SELECT * FROM Test;";
        var result = await dockerService.ExecInContainer(
            container.Name,
            [
                "mysql",
                "-u", mysqlDatabase.User,
                $"-p{mysqlDatabase.Password}",
                "-h", mysqlDatabase.Host,
                "-e", sql,
                mysqlDatabase.Database
            ]
        );
        
        Assert.Contains("246", result.stdout);

        // clean up
        UninstallApplication(application);
        UninstallApplication(mysqlApplication);
    }
}