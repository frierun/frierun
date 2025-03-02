using Frierun.Server.Data;
using Frierun.Server.Services;

namespace Frierun.Tests.Installers;

public class MysqlInstallerTests : BaseTests
{
    private readonly InstallerRegistry _installerRegistry;
    private readonly Application _application;

    /// <inheritdoc />
    public MysqlInstallerTests()
    {
        Resolve<PackageRegistry>().Load();
        var package = Resolve<PackageRegistry>().Find("mysql")
                      ?? throw new Exception("Mysql package not found");

        _installerRegistry = Resolve<InstallerRegistry>();
        Assert.Empty(_installerRegistry.GetInstallers(typeof(Mysql), "MysqlInstaller"));

        _application = InstallPackage(package)
                       ?? throw new Exception("Mysql application not installed");
    }
    
    [Fact]
    public void Install_MysqlPackage_AddsMysqlInstaller()
    {
        Assert.Single(_installerRegistry.GetInstallers(typeof(Mysql), "MysqlInstaller"));
        Assert.NotNull(_installerRegistry.GetUninstaller(typeof(MysqlDatabase)));
    }

    [Fact]
    public void Uninstall_MysqlPackage_RemovesMysqlInstaller()
    {
        Resolve<UninstallService>().Handle(_application);

        Assert.Empty(_installerRegistry.GetInstallers(typeof(Mysql), "MysqlInstaller"));
        Assert.Null(_installerRegistry.GetUninstaller(typeof(MysqlDatabase)));
    }
    
    [Fact]
    public void Install_PackageWithMysql_CreatesMysqlDatabase()
    {
        var package = Factory<Package>().Generate() with
        {
            Contracts =
            [
                new Mysql(),
            ]
        };

        var application = InstallPackage(package);

        Assert.NotNull(application);
        var mysqlDatabase = application.DependsOn.OfType<MysqlDatabase>().First();
        Assert.Equal(package.Name, mysqlDatabase.User);
        Assert.Equal(package.Name, mysqlDatabase.Database);
        Assert.Single(mysqlDatabase.DependsOn.OfType<DockerNetwork>());
        Assert.Equal([_application], mysqlDatabase.DependsOn.OfType<Application>());
    }    
}