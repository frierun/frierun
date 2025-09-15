using Bogus;
using Frierun.Server.Data;

namespace Frierun.Tests.Handlers.Base;

public class ApplicationHandlerTests : BaseTests
{
    public ApplicationHandlerTests()
    {
        InstallPackage("docker");
    }

    [Fact]
    public void Install_CompletePackage_ApplicationUrlHasPriority()
    {
        var package = Factory<Package>().Generate() with
        {
            Contracts = new ContractList
            {
                ["http"] = new HttpEndpoint(Port: 80),
                ["tcp"] = new PortEndpoint(Protocol.Tcp, 2222)
            }
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
            Contracts = new ContractList
            {
                ["http"] = new HttpEndpoint(Port: 80),
                ["tcp"] = new PortEndpoint(Protocol.Tcp, 2222)
            }
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
            Contracts = new ContractList
            {
                ["tcp"] = new PortEndpoint(Protocol.Tcp, 2222)
            }
        };

        var application = InstallPackage(package);

        Assert.Equal("tcp://127.0.0.1:2222", application.Url);
    }

    [Fact]
    public void Install_ApplicationUrlWithTemplate_ResolvesTemplate()
    {
        var value = Resolve<Faker>().Lorem.Word();
        var parameter = Contract<Parameter>().Set(p => p.Value, value).Generate();
        var package = Factory<Package>().Generate() with
        {
            ApplicationUrl = new Argument<string>($"{{{{{parameter.Id}:Value}}}}"),
            Contracts = [parameter]
        };

        var application = InstallPackage(package);

        Assert.Equal(value, application.Url);
    }

    [Fact]
    public void Install_ApplicationDescriptionWithTemplate_ResolvesTemplate()
    {
        var value = Resolve<Faker>().Lorem.Word();
        var parameter = Contract<Parameter>().Set(p => p.Value, value).Generate();
        var package = Factory<Package>().Generate() with
        {
            ApplicationDescription = new Argument<string>($"{{{{{parameter.Id}:Value}}}}"),
            Contracts = [parameter]
        };

        var application = InstallPackage(package);

        Assert.Equal(value, application.Description);
    }
}