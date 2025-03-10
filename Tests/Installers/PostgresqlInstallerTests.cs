using Frierun.Server;
using Frierun.Server.Data;

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
        var database = application.DependsOn.OfType<PostgresqlDatabase>().First();
        Assert.Equal(package.Name, database.User);
        Assert.Equal(package.Name, database.Database);
        Assert.Single(database.DependsOn.OfType<DockerNetwork>());
        Assert.Equal([providerApplication], database.DependsOn.OfType<Application>());
    }    
}