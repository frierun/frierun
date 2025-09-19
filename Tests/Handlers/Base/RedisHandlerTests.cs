using Frierun.Server.Data;

namespace Frierun.Tests.Handlers.Base;

public class RedisHandlerTests : BaseTests
{
    [Fact]
    public void Install_PackageWithContract_CreatesDatabase()
    {
        InstallPackage("docker");
        var package = Factory<Package>().Generate() with { Contracts = new ContractList { [""] = new Redis() } };

        var application = InstallPackage(package);

        var database = Resolve<State>().GetContract<Redis>(application);
        Assert.True(database.Installed);

        var dbContainer = Resolve<State>().GetContract(application, database.Container);
        Assert.True(dbContainer.Installed);
        Assert.Equal(database.Host, dbContainer.ContainerName);
    }
}