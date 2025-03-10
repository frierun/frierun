using Frierun.Server;
using Frierun.Server.Data;
using Frierun.Server.Services;

namespace Frierun.Tests.Installers;

public class MysqlInstallerTests : BaseTests
{
   
    [Fact]
    public void Install_PackageWithContract_CreatesDatabase()
    {
        var providerApplication = InstallPackage("mysql") 
                       ?? throw new Exception("Mysql application not installed");
        
        var package = Factory<Package>().Generate() with
        {
            Contracts =
            [
                new Mysql(),
            ]
        };

        var application = InstallPackage(package);

        Assert.NotNull(application);
        var database = application.DependsOn.OfType<MysqlDatabase>().First();
        Assert.Equal(package.Name, database.User);
        Assert.Equal(package.Name, database.Database);
        Assert.Single(database.DependsOn.OfType<DockerNetwork>());
        Assert.Equal([providerApplication], database.DependsOn.OfType<Application>());
    }    
}