using Frierun.Server.Data;

namespace Frierun.Tests.Installers.Base;

public class RedisInstallerTests : BaseTests
{
    [Fact]
    public void Install_PackageWithContract_CreatesDatabase()
    {
        TryInstallPackage("docker");
        var package = Factory<Package>().Generate() with { Contracts = [new Redis()] };

        var application = TryInstallPackage(package);

        Assert.NotNull(application);
        Assert.Single(application.Resources.OfType<RedisDatabase>());
        var database = application.Resources.OfType<RedisDatabase>().First();

        Assert.Single(application.Resources.OfType<DockerContainer>());
        var dbContainer = application.Resources.OfType<DockerContainer>().First();
        Assert.Equal(database.Host, dbContainer.Name);
    }
}