using Frierun.Server.Data;

namespace Frierun.Tests.Installers.Base;

public class RedisInstallerTests : BaseTests
{
   
    [Fact]
    public void Install_PackageWithContract_CreatesDatabase()
    {
        var package = Factory<Package>().Generate() with
        {
            Contracts =
            [
                new Redis(),
            ]
        };

        var application = InstallPackage(package);

        Assert.NotNull(application);
        var database = application.DependsOn.OfType<RedisDatabase>().First();
        var dbContainer = database.DependsOn.OfType<DockerContainer>().First();
        Assert.Equal(database.Host, dbContainer.Name);
    }    
}