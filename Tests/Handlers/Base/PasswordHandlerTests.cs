using Frierun.Server.Data;

namespace Frierun.Tests.Handlers.Base;

public class PasswordHandlerTests : BaseTests
{
    [Fact]
    public void Install_Password_CreatesRandomString()
    {
        var password = Contract<Password>().Generate();
        var package = Factory<Package>().Generate() with { Contracts = [password] };

        var application = InstallPackage(package);

        var installedPassword = application.GetContract(password.Id);
        Assert.True(installedPassword.Installed);
        Assert.NotNull(installedPassword.Value);
    }

    [Fact]
    public void Install_Password_CanBeInserted()
    {
        var password = Contract<Password>().Generate();
        var package = Factory<Package>().Generate() with
        {
            ApplicationDescription = $"GeneratedPassword: {{{{{password.Id}:Value}}}}",
            Contracts = [password]
        };

        var application = InstallPackage(package);

        var installedPassword = application.GetContract(password.Id);
        Assert.True(installedPassword.Installed);
        Assert.Equal(application.Description, $"GeneratedPassword: {installedPassword.Value}");
    }
}