using Frierun.Server.Data;
using Frierun.Server.Handlers;
using Frierun.Server.Handlers.Base;

namespace Frierun.Tests.Handlers.Base;

public class SelectorHandlerTests : BaseTests
{
    [Fact]
    public void Initialize_WithSelectedOption_ReturnsSingleOption()
    {
        var (container1Id, container1) = Contract<Container>().GenerateEntry();
        var (container2Id, container2) = Contract<Container>().GenerateEntry();
        var (selectorId, selector) = Contract<Selector>().GenerateEntry();
        selector = selector with
        {
            Options =
            [
                new SelectorOption("option1", new ContractList { [container1Id] = container1 }),
                new SelectorOption("option2", new ContractList { [container2Id] = container2 })
            ],
            Value = "option2"
        };
        var handler = Handler<SelectorHandler>();

        var result = handler.Initialize(selector, new ApplicationContext(selectorId, "prefix")).ToList();

        Assert.Single(result);
        Assert.Equal("option2", ((Selector)result[0][selectorId]).Value);
        Assert.Equal(2, result[0].Count);
        Assert.Equal(container2, result[0][container2Id]);
    }

    [Fact]
    public void Initialize_WithoutSelectedOption_ReturnsAllOptions()
    {
        var (container1Id, container1) = Contract<Container>().GenerateEntry();
        var (container2Id, container2) = Contract<Container>().GenerateEntry();
        var (selectorId, selector) = Contract<Selector>().GenerateEntry();
        selector = selector with
        {
            Options =
            [
                new SelectorOption("option1", new ContractList { [container1Id] = container1 }),
                new SelectorOption("option2", new ContractList { [container2Id] = container2 })
            ],
        };
        var handler = Handler<SelectorHandler>();

        var result = handler.Initialize(selector, new ApplicationContext(selectorId, "prefix")).ToList();

        Assert.Equal(2, result.Count);
        Assert.Equal("option1", ((Selector)result[0][selectorId]).Value);
        Assert.Equal(2, result[0].Count);
        Assert.Equal(container1, result[0][container1Id]);

        Assert.Equal("option2", ((Selector)result[1][selectorId]).Value);
        Assert.Equal(2, result[1].Count);
        Assert.Equal(container2, result[1][container2Id]);
    }

    [Fact]
    public void Install_PackageWithSelector_PackageDependsOnSelectorChildren()
    {
        var (contractId, contract) = Contract<Parameter>().GenerateEntry();
        var (selectorId, selector) = Contract<Selector>().GenerateEntry();
        selector = selector with
        {
            Options =
            [
                new SelectorOption("option1", new ContractList { [contractId] = contract }),
            ]
        };
        var package = Factory<Package>().Generate() with { Contracts = new ContractList { [selectorId] = selector } };

        var application = InstallPackage(package);

        var installedSelector = application.GetContract(contractId);
        Assert.True(installedSelector.Installed);
        Assert.Equal(contract.Value, installedSelector.Value);
    }
}