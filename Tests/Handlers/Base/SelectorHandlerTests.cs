using Frierun.Server.Data;
using Frierun.Server.Handlers;
using Frierun.Server.Handlers.Base;

namespace Frierun.Tests.Handlers.Base;

public class SelectorHandlerTests : BaseTests
{
    [Fact]
    public void Initialize_WithSelectedOption_ReturnsSingleOption()
    {
        var container1 = Factory<Container>().Generate();
        var container2 = Factory<Container>().Generate();
        var selector = new Selector(
            "selector",
            [
                new SelectorOption("option1", [container1]),
                new SelectorOption("option2", [container2])
            ],
            "option2"
        );
        var handler = Handler<SelectorHandler>();

        var result = handler.Initialize(selector, new ApplicationContext(selector.Id.Name, "prefix")).ToList();

        Assert.Single(result);
        Assert.Equal("option2", ((Selector)result[0][selector.Id]).Value);
        Assert.Equal(2, result[0].Count);
        Assert.Equal(container2, result[0][container2.Id]);
    }

    [Fact]
    public void Initialize_WithoutSelectedOption_ReturnsAllOptions()
    {
        var container1 = Factory<Container>().Generate();
        var container2 = Factory<Container>().Generate();
        var selector = new Selector(
            "selector", [
                new SelectorOption("option1", [container1]),
                new SelectorOption("option2", [container2])
            ]
        );
        var handler = Handler<SelectorHandler>();

        var result = handler.Initialize(selector, new ApplicationContext(selector.Id.Name, "prefix")).ToList();

        Assert.Equal(2, result.Count);
        Assert.Equal("option1", ((Selector)result[0][selector.Id]).Value);
        Assert.Equal(2, result[0].Count);
        Assert.Equal(container1, result[0][container1.Id]);

        Assert.Equal("option2", ((Selector)result[1][selector.Id]).Value);
        Assert.Equal(2, result[1].Count);
        Assert.Equal(container2, result[1][container2.Id]);
    }

    [Fact]
    public void Install_PackageWithSelector_PackageDependsOnSelectorChildren()
    {
        var contract = Factory<Parameter>().Generate();
        var selector = new Selector(
            "selector", [
                new SelectorOption("option1", [contract]),
            ]
        );
        var package = Factory<Package>().Generate() with { Contracts = [selector] };

        var application = InstallPackage(package);

        var installedSelector = application.GetContract(new ContractId<Parameter>(contract.Id));
        Assert.True(installedSelector.Installed);
        Assert.Equal(contract.Value, installedSelector.Value);
    }
}