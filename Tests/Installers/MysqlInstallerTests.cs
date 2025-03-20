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
        Assert.Single(database.DependsOn.OfType<DockerAttachedNetwork>());
        Assert.Equal([providerApplication], database.DependsOn.OfType<Application>());
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
        var databases = application.DependsOn.OfType<MysqlDatabase>().ToList();
        Assert.Equal(2, databases.Count);
        
        var attachedNetworks = databases.SelectMany(d => d.DependsOn.OfType<DockerAttachedNetwork>()).ToList();
        Assert.Equal(2, attachedNetworks.Count);
        Assert.Equal(attachedNetworks[0], attachedNetworks[1]);
    }    
}