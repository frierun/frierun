using Frierun.Server.Data;

namespace Frierun.Tests.Installers.Base;

public class DependencyInstallerTests : BaseTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Install_TwoContactsWithDependency_CorrectOrder(bool reverseOrder)
    {
        var contracts = Factory<Parameter>().Generate(2);
        var package = Factory<Package>().Generate() with
        {
            Contracts = [
                reverseOrder ? contracts[1] : contracts[0],
                reverseOrder ? contracts[0] : contracts[1],
                new Dependency(contracts[0].Id, contracts[1].Id)
            ]
        };

        var application = InstallPackage(package);

        var installedContracts = application.Contracts.OfType<Parameter>();
        Assert.Equal(
            [contracts[0].Name, contracts[1].Name],
            installedContracts.Select(c => c.Name)
        );
    }
}