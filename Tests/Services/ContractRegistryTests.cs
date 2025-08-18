using Bogus;
using Frierun.Server;
using Frierun.Server.Data;

namespace Frierun.Tests;

public class ContractRegistryTests : BaseTests
{
    [Theory]
    [InlineData(typeof(Container))]
    [InlineData(typeof(HttpEndpoint))]
    public void GetContractType_ExistingType_ReturnsCorrectType(Type type)
    {
        var contractRegistry = Resolve<ContractRegistry>();

        var resultingType = ContractRegistry.GetContractType(type.Name);

        Assert.Same(type, resultingType);
    }
    
    [Fact]
    public void GetContractType_NonExistingType_ThrowsException()
    {
        var contractRegistry = Resolve<ContractRegistry>();

        Assert.Throws<Exception>(() => ContractRegistry.GetContractType("NonExistingType"));
    }
    
    [Theory]
    [InlineData(typeof(Container))]
    [InlineData(typeof(HttpEndpoint))]
    [InlineData(typeof(Mysql))]
    [InlineData(typeof(Network))]
    [InlineData(typeof(Package))]
    [InlineData(typeof(Parameter))]
    [InlineData(typeof(Password))]
    [InlineData(typeof(Postgresql))]
    [InlineData(typeof(Redis))]
    [InlineData(typeof(Volume))]
    public void CreateContract_ExistingType_ReturnsCorrectContract(Type type)
    {
        var contractRegistry = Resolve<ContractRegistry>();
        var faker = Resolve<Faker>();
        var name = faker.Lorem.Word();

        var contract = ContractRegistry.CreateContract(type.Name, name);

        Assert.IsType(type, contract);
        Assert.Equal(name, contract.Name);
    }
    
    [Theory]
    [InlineData("NonExistingType")]
    public void CreateContract_InvalidType_ThrowsException(string invalidType)
    {
        var contractRegistry = Resolve<ContractRegistry>();
        var faker = Resolve<Faker>();
        var name = faker.Lorem.Word();

        Assert.Throws<Exception>(() => ContractRegistry.CreateContract(invalidType, name));
    }
}