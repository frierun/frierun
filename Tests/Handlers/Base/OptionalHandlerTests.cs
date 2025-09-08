using Frierun.Server.Data;
using Frierun.Server.Handlers;
using Frierun.Server.Handlers.Base;

namespace Frierun.Tests.Handlers.Base;

public class OptionalHandlerTests : BaseTests
{
    [Fact]
    public void Initialize_WithoutSelectedOption_ReturnsBothOptions()
    {
        var (containerId, container) = Contract<Container>().GenerateEntry();
        var (contractId, contract) = Contract<Optional>().GenerateEntry();
        contract = contract with { Contracts = new ContractList { [containerId] = container } };
        var handler = Handler<OptionalHandler>();

        var result = handler.Initialize(contract, new ApplicationContext(contractId, "prefix")).ToList();

        Assert.Equal(2, result.Count);

        Assert.Equal(true, ((Optional)result[0][contractId]).Value);
        Assert.Equal(2, result[0].Count);
        Assert.Equal(container, result[0][containerId]);

        Assert.Equal(false, ((Optional)result[1][contractId]).Value);
        Assert.Single(result[1]);
    }

    [Fact]
    public void Initialize_WithOptionSelected_ReturnsContract()
    {
        var (containerId, container) = Contract<Container>().GenerateEntry();
        var (contractId, contract) = Contract<Optional>().GenerateEntry();
        contract = contract with { Value = true, Contracts = new ContractList { [containerId] = container } };
        var handler = Handler<OptionalHandler>();

        var result = handler.Initialize(contract, new ApplicationContext(contractId, "prefix")).ToList();

        Assert.Single(result);
        Assert.Equal(true, ((Optional)result[0][contractId]).Value);
        Assert.Equal(2, result[0].Count);
        Assert.Equal(container, result[0][containerId]);
    }
    
    [Fact]
    public void Initialize_WithOptionDeselected_ReturnsEmptyList()
    {
        var (containerId, container) = Contract<Container>().GenerateEntry();
        var (contractId, contract) = Contract<Optional>().GenerateEntry();
        contract = contract with { Value = false, Contracts = new ContractList { [containerId] = container } };
        var handler = Handler<OptionalHandler>();

        var result = handler.Initialize(contract, new ApplicationContext(contractId, "prefix")).ToList();

        Assert.Single(result);
        Assert.Equal(false, ((Optional)result[0][contractId]).Value);
        Assert.Single(result[0]);        
    }    
}