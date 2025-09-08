using Frierun.Server.Data;

namespace Frierun.Tests.Handlers.Base;

public class PasswordHandlerTests : BaseTests
{
    [Fact]
    public void Install_Password_CreatesRandomString()
    {
        var (passwordId, password) = Contract<Password>().GenerateEntry();
        var package = Factory<Package>().Generate() with
        {
            Contracts = new ContractList { [passwordId] = password }
        };

        var application = InstallPackage(package);

        var installedPassword = application.GetContract(passwordId);
        Assert.True(installedPassword.Installed);
        Assert.NotNull(installedPassword.Value);
    }

    [Fact]
    public void Install_Password_CanBeInserted()
    {
        var (passwordId, password) = Contract<Password>().GenerateEntry();
        var package = Factory<Package>().Generate() with
        {
            ApplicationDescription = $"GeneratedPassword: {{{{Password:{passwordId.Name}:Value}}}}",
            Contracts = new ContractList { [passwordId] = password }
        };

        var application = InstallPackage(package);

        var installedPassword = application.GetContract(passwordId);
        Assert.True(installedPassword.Installed);
        Assert.Equal(application.Description, $"GeneratedPassword: {installedPassword.Value}");
    }
}