using Frierun.Server.Data;

namespace Frierun.Tests.Handlers.Base;

public class ParameterHandlerTests : BaseTests
{
    [Fact]
    public void Install_WithDefaultValue_SetsDefaultValue()
    {
        var parameter = Contract<Parameter>()
            .Set(p => p.Value, new Argument<string>())
            .Generate();
        var package = Factory<Package>().Generate() with { Contracts = [parameter] };
        Assert.NotNull(parameter.Contract.DefaultValue);

        var application = InstallPackage(package);

        var installedParameter = application.GetContract(parameter.Id);
        Assert.Equal(parameter.Contract.DefaultValue, installedParameter.Value.Value);
    }
}