using Frierun.Server.Data;

namespace Frierun.Tests.Handlers.Base;

public class PasswordHandlerTests : BaseTests
{
    [Fact]
    public void Install_Password_CreatesRandomString()
    {
        var package = Factory<Package>().Generate() with
        {
            Contracts = [new Password()]
        };

        var application = InstallPackage(package);

        var password = application.Contracts.OfType<Password>().Single();
        Assert.True(password.Installed);
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

        var application = InstallPackage(package);

        var password = application.Contracts.OfType<Password>().Single();
        Assert.True(password.Installed);
        Assert.Equal(application.Description, $"GeneratedPassword: {password.Value}");
    }
}