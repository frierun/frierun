using Bogus;
using Frierun.Server.Data;

namespace Frierun.Tests.Handlers.Base;

public class PackageHandlerTests : BaseTests
{
    public PackageHandlerTests()
    {
        InstallPackage("docker");
    }

    [Fact]
    public void Install_CompletePackage_ApplicationUrlHasPriority()
    {
        var package = Factory<Package>().Generate() with
        {
            Contracts =
            [
                new HttpEndpoint(Port: 80),
                new PortEndpoint(
                    Protocol.Tcp,
                    2222
                )
            ]
        };
        Assert.NotNull(package.ApplicationUrl.Value);

        var application = InstallPackage(package);

        Assert.Equal(package.ApplicationUrl.Value, application.Url);
    }

    [Fact]
    public void Install_PackageWithHttpAndPortEndpoint_HttpEndpointHasPriority()
    {
        var package = Factory<Package>().Generate() with
        {
            ApplicationUrl = new Argument<string>(),
            Contracts =
            [
                new HttpEndpoint(Port: 80),
                new PortEndpoint(
                    Protocol.Tcp,
                    2222
                )
            ]
        };

        var application = InstallPackage(package);

        Assert.Equal("http://127.0.0.1/", application.Url);
    }

    [Fact]
    public void Install_PackageWithPortEndpoint_AutoDetectPortEndpoint()
    {
        var package = Factory<Package>().Generate() with
        {
            ApplicationUrl = new Argument<string>(),
            Contracts =
            [
                new PortEndpoint(
                    Protocol.Tcp,
                    2222
                )
            ]
        };

        var application = InstallPackage(package);

        Assert.Equal("tcp://127.0.0.1:2222", application.Url);
    }

    [Fact]
    public void Install_ApplicationUrlWithTemplate_ResolvesTemplate()
    {
        var value = Resolve<Faker>().Lorem.Word();
        var package = Factory<Package>().Generate() with
        {
            ApplicationUrl = new Argument<string>("{{Parameter:Test:Value}}"),
            Contracts = [new Parameter("Test", Value: value)]
        };

        var application = InstallPackage(package);

        Assert.Equal(value, application.Url);
    }

    [Fact]
    public void Install_ApplicationDescriptionWithTemplate_ResolvesTemplate()
    {
        var value = Resolve<Faker>().Lorem.Word();
        var package = Factory<Package>().Generate() with
        {
            ApplicationDescription = new Argument<string>("{{Parameter:Test:Value}}"),
            Contracts = [new Parameter("Test", Value: value)]
        };

        var application = InstallPackage(package);

        Assert.Equal(value, application.Description);
    }
}