using Frierun.Server.Data;
using Frierun.Server.Installers;
using Frierun.Server.Installers.Base;

namespace Frierun.Tests.Installers.Base;

public class SelectorInstallerTests : BaseTests
{
    [Fact]
    public void Initialize_WithSelectedOption_ReturnsSingleOption()
    {
        var selector = new Selector(
            "selector", [
                new SelectorOption("option1", [new Container("container1")]),
                new SelectorOption("option2", [new Container("container2")])
            ], "option2"
        );
        IInstaller installer = new SelectorInstaller();

        var result = installer.Initialize(selector, "prefix").ToList();

        // Assert
        Assert.Single(result);
        var resolvedContract = (Selector)result[0].Contract;
        Assert.Equal("option2", resolvedContract.SelectedOption);
        Assert.Single(result[0].AdditionalContracts);
        Assert.Equal("container2", result[0].AdditionalContracts.First().Name);
    }

    [Fact]
    public void Initialize_WithoutSelectedOption_ReturnsAllOptions()
    {
        var selector = new Selector(
            "selector", [
                new SelectorOption("option1", [new Container("container1")]),
                new SelectorOption("option2", [new Container("container2")])
            ]
        );
        IInstaller installer = new SelectorInstaller();

        var result = installer.Initialize(selector, "prefix").ToList();

        Assert.Equal(2, result.Count);
        var resolvedContract1 = (Selector)result[0].Contract;
        Assert.Equal("option1", resolvedContract1.SelectedOption);
        Assert.Single(result[0].AdditionalContracts);
        Assert.Equal("container1", result[0].AdditionalContracts.First().Name);

        var resolvedContract2 = (Selector)result[1].Contract;
        Assert.Equal("option2", resolvedContract2.SelectedOption);
        Assert.Single(result[1].AdditionalContracts);
        Assert.Equal("container2", result[1].AdditionalContracts.First().Name);
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
        var package = Factory<Package>().Generate() with { Contracts = new List<Contract> { selector } };

        var application = TryInstallPackage(package);

        Assert.NotNull(application);
        var resolvedParameter = application.Resources.OfType<ResolvedParameter>().First(p => p.Name == contract.Name);
        Assert.Equal(contract.Value, resolvedParameter.Value);
    }
}