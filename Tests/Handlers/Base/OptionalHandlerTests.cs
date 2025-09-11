using Frierun.Server.Data;
using Frierun.Server.Handlers;
using Frierun.Server.Handlers.Base;

namespace Frierun.Tests.Handlers.Base;

public class OptionalHandlerTests : BaseTests
{
    [Fact]
    public void Initialize_WithoutSelectedOption_ReturnsBothOptions()
    {
        var container = Contract<Container>().Generate();
        var optional = Contract<Optional>()
            .Set(p => p.Contracts, [container])
            .Generate();
        var handler = Handler<OptionalHandler>();

        var result = handler.Initialize(optional.Contract, new ApplicationContext(optional.Id, "prefix")).ToList();

        Assert.Equal(2, result.Count);

        Assert.Equal(true, ((Optional)result[0][optional.Id]).Value);
        Assert.Equal(2, result[0].Count);
        Assert.Equal(container.Contract, result[0][container.Id]);

        Assert.Equal(false, ((Optional)result[1][optional.Id]).Value);
        Assert.Single(result[1]);
    }

    [Fact]
    public void Initialize_WithOptionSelected_ReturnsContract()
    {
        var container = Contract<Container>().Generate();
        var optional = Contract<Optional>()
            .Set<bool?>(p => p.Value, true)
            .Set(p => p.Contracts, [container])
            .Generate();
        var handler = Handler<OptionalHandler>();

        var result = handler.Initialize(optional.Contract, new ApplicationContext(optional.Id, "prefix")).ToList();

        Assert.Single(result);
        Assert.Equal(true, ((Optional)result[0][optional.Id]).Value);
        Assert.Equal(2, result[0].Count);
        Assert.Equal(container.Contract, result[0][container.Id]);
    }

    [Fact]
    public void Initialize_WithOptionDeselected_ReturnsEmptyList()
    {
        var container = Contract<Container>().Generate();
        var optional = Contract<Optional>()
            .Set<bool?>(p => p.Value, false)
            .Set(p => p.Contracts, [container])
            .Generate();
        var handler = Handler<OptionalHandler>();

        var result = handler.Initialize(optional.Contract, new ApplicationContext(optional.Id, "prefix")).ToList();

        Assert.Single(result);
        Assert.Equal(false, ((Optional)result[0][optional.Id]).Value);
        Assert.Single(result[0]);
    }
}