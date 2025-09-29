using Frierun.Server.Data;

namespace Frierun.Tests.Data;

public class ContractListTests : BaseTests
{
    [Fact]
    public void Merge_WithEmptyContract_ReturnsContract()
    {
        var container = Contract<Container>().Generate();
        var contractList = new ContractList {[container.Ref] = container.Contract};
        var emptyContractList = new ContractList {[container.Ref] = new Container()};
        
        var result = contractList.Merge(emptyContractList);
        
        var resultContainer = result[container.Ref] as Container;
        Assert.NotNull(resultContainer);
        Assert.Equal(container.Contract.ImageName, resultContainer.ImageName);
        Assert.Equal(container.Contract.NetworkName, resultContainer.NetworkName);
        Assert.Equal(container.Contract.ContainerName, resultContainer.ContainerName);
        Assert.Equal(container.Contract.Command, resultContainer.Command);
        Assert.Equal(container.Contract.Network, resultContainer.Network);
    }
    
    [Fact]
    public void Merge_EmptyWithContract_ReturnsContract()
    {
        var container = Contract<Container>().Generate();
        var contractList = new ContractList {[container.Ref] = container.Contract};
        var emptyContractList = new ContractList {[container.Ref] = new Container()};
        
        var result = emptyContractList.Merge(contractList);
        
        var resultContainer = result[container.Ref] as Container;
        Assert.NotNull(resultContainer);
        Assert.Equal(container.Contract.ImageName, resultContainer.ImageName);
        Assert.Equal(container.Contract.NetworkName, resultContainer.NetworkName);
        Assert.Equal(container.Contract.ContainerName, resultContainer.ContainerName);
        Assert.Equal(container.Contract.Command, resultContainer.Command);
        Assert.Equal(container.Contract.Network, resultContainer.Network);
    }
    
    [Fact]
    public void Merge_WithInstalledContract_ReturnsContract()
    {
        var contract = Contract<DockerApiConnection>().Generate("installed");
        var contractList = new ContractList {[contract.Ref] = contract.Contract};
        var emptyContractList = new ContractList {[contract.Ref] = new DockerApiConnection()};
        
        var result = contractList.Merge(emptyContractList);
        
        var resultContract = result[contract.Ref] as DockerApiConnection;
        Assert.NotNull(resultContract);
        Assert.Equal(contract.Contract.Path, resultContract.Path);
        Assert.Equal(contract.Contract.IsPodman, resultContract.IsPodman);
    }
    
}