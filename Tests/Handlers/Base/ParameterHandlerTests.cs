using Frierun.Server.Data;

namespace Frierun.Tests.Handlers.Base;

public class ParameterHandlerTests : BaseTests
{
    [Fact]
    public void Install_WithDefaultValue_SetsDefaultValue()
    {
        var parameter = Factory<Parameter>().Generate() with { Value = new Argument<string>()};
        Assert.NotNull(parameter.DefaultValue);
        var package = Factory<Package>().Generate() with { Contracts = [parameter] };
        
        var application = InstallPackage(package);
        
        var installedParameter = application.GetContract(new ContractId<Parameter>(parameter.Id));
        Assert.Equal(parameter.DefaultValue, installedParameter.Value.Value);
    }
}