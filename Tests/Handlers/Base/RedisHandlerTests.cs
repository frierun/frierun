using Frierun.Server.Data;

namespace Frierun.Tests.Handlers.Base;

public class RedisHandlerTests : BaseTests
{
    [Fact]
    public void Install_PackageWithContract_CreatesDatabase()
    {
        InstallPackage("docker");
        var package = Factory<Package>().Generate() with { Contracts = [new Redis()] };

        var application = InstallPackage(package);

        var database = application.Contracts.OfType<Redis>().Single();
        Assert.True(database.Installed);

        var dbContainer = application.Contracts.OfType<Container>().Single();
        Assert.True(dbContainer.Installed);
        Assert.Equal(database.Host, dbContainer.ContainerName);
    }
}