using Frierun.Server.Data;
using Frierun.Server.Handlers;
using Frierun.Server.Handlers.Base;

namespace Frierun.Tests.Handlers.Base;

public class SelectorHandlerTests : BaseTests
{
    [Fact]
    public void Initialize_WithSelectedOption_ReturnsSingleOption()
    {
        var container1 = Contract<Container>().Generate();
        var container2 = Contract<Container>().Generate();
        var selector = Contract<Selector>()
            .Set(
                p => p.Options,
                [
                    new SelectorOption("option1", [container1]),
                    new SelectorOption("option2", [container2])
                ]
            )
            .Set(p => p.Value, "option2")
            .Generate();
        var handler = Handler<SelectorHandler>();

        var result = handler.Initialize(selector.Contract, new ApplicationContext(selector.Id, "prefix")).ToList();

        Assert.Single(result);
        Assert.Equal("option2", ((Selector)result[0][selector.Id]).Value);
        Assert.Equal(2, result[0].Count);
        Assert.Equal(container2.Contract, result[0][container2.Id]);
    }

    [Fact]
    public void Initialize_WithoutSelectedOption_ReturnsAllOptions()
    {
        var container1 = Contract<Container>().Generate();
        var container2 = Contract<Container>().Generate();
        var selector = Contract<Selector>()
            .Set(
                p => p.Options,
                [
                    new SelectorOption("option1", [container1]),
                    new SelectorOption("option2", [container2])
                ]
            )
            .Generate();
        var handler = Handler<SelectorHandler>();

        var result = handler.Initialize(selector.Contract, new ApplicationContext(selector.Id, "prefix")).ToList();

        Assert.Equal(2, result.Count);
        Assert.Equal("option1", ((Selector)result[0][selector.Id]).Value);
        Assert.Equal(2, result[0].Count);
        Assert.Equal(container1.Contract, result[0][container1.Id]);

        Assert.Equal("option2", ((Selector)result[1][selector.Id]).Value);
        Assert.Equal(2, result[1].Count);
        Assert.Equal(container2.Contract, result[1][container2.Id]);
    }

    [Fact]
    public void Install_PackageWithSelector_PackageDependsOnSelectorChildren()
    {
        var contract = Contract<Parameter>().Generate();
        var selector = Contract<Selector>()
            .Set(p => p.Options, [new SelectorOption("option", [contract]),])
            .Generate();
        var package = Factory<Package>().Generate() with { Contracts = [selector] };

        var application = InstallPackage(package);

        var installedSelector = application.GetContract(contract.Id);
        Assert.True(installedSelector.Installed);
        Assert.Equal(contract.Contract.Value, installedSelector.Value);
    }
}