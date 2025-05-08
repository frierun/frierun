using Frierun.Server.Data;

namespace Frierun.Tests.Installers.Base;

public class PasswordInstallerTests : BaseTests
{
    [Fact]
    public void Install_Password_CreatesRandomString()
    {
        var package = Factory<Package>().Generate() with
        {
            Contracts = [new Password()]
        };

        var application = TryInstallPackage(package);

        Assert.NotNull(application);
        var password = application.Resources.OfType<GeneratedPassword>().FirstOrDefault();
        Assert.NotNull(password);
        Assert.NotNull(password.Value);
    }

    [Fact]
    public void Install_Password_CanBeInserted()
    {
        var package = Factory<Package>().Generate() with
        {
            ApplicationDescription = "GeneratedPassword: {{Password::Value}}",
            Contracts = [new Password()]
        };

        var application = TryInstallPackage(package);

        Assert.NotNull(application);
        var password = application.Resources.OfType<GeneratedPassword>().FirstOrDefault();
        Assert.NotNull(password);
        Assert.Equal(application.Description, $"GeneratedPassword: {password.Value}");
    }
}