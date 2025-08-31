using Frierun.Server.Data;
using Frierun.Server.Handlers;
using Frierun.Server.Handlers.Base;

namespace Frierun.Tests.Handlers.Base;

public class OptionalHandlerTests : BaseTests
{
    [Fact]
    public void Initialize_WithoutSelectedOption_ReturnsBothOptions()
    {
        var container = Factory<Container>().Generate();
        var contract = new Optional("", [container]);
        var handler = Handler<OptionalHandler>();

        var result = handler.Initialize(contract, new ApplicationContext("", "prefix")).ToList();

        Assert.Equal(2, result.Count);

        Assert.Equal(true, ((Optional)result[0][contract.Id]).Value);
        Assert.Equal(2, result[0].Count);
        Assert.Equal(container, result[0][container.Id]);

        Assert.Equal(false, ((Optional)result[1][contract.Id]).Value);
        Assert.Single(result[1]);
    }

    [Fact]
    public void Initialize_WithOptionSelected_ReturnsContract()
    {
        var container = Factory<Container>().Generate();
        var contract = new Optional("", [container]) { Value = true };
        var handler = Handler<OptionalHandler>();

        var result = handler.Initialize(contract, new ApplicationContext("", "prefix")).ToList();

        Assert.Single(result);
        Assert.Equal(true, ((Optional)result[0][contract.Id]).Value);
        Assert.Equal(2, result[0].Count);
        Assert.Equal(container, result[0][container.Id]);
    }
    
    [Fact]
    public void Initialize_WithOptionDeselected_ReturnsEmptyList()
    {
        var container = Factory<Container>().Generate();
        var contract = new Optional("", [container]) { Value = false };
        var handler = Handler<OptionalHandler>();

        var result = handler.Initialize(contract, new ApplicationContext("", "prefix")).ToList();

        Assert.Single(result);
        Assert.Equal(false, ((Optional)result[0][contract.Id]).Value);
        Assert.Single(result[0]);        
    }    
}