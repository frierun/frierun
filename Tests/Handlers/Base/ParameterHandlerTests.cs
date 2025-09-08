using Frierun.Server.Data;

namespace Frierun.Tests.Handlers.Base;

public class ParameterHandlerTests : BaseTests
{
    [Fact]
    public void Install_WithDefaultValue_SetsDefaultValue()
    {
        var (parameterId, parameter) = Contract<Parameter>().GenerateEntry();
        parameter = parameter with { Value = new Argument<string>() };
        var package = Factory<Package>().Generate() with { Contracts = new ContractList { [parameterId] = parameter } };
        Assert.NotNull(parameter.DefaultValue);

        var application = InstallPackage(package);

        var installedParameter = application.GetContract(new ContractId<Parameter>(parameterId));
        Assert.Equal(parameter.DefaultValue, installedParameter.Value.Value);
    }
}