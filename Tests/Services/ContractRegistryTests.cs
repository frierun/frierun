using Bogus;
using Frierun.Server;
using Frierun.Server.Data;
using Frierun.Server.Services;
using File = Frierun.Server.Data.File;

namespace Frierun.Tests.Services;

public class ContractRegistryTests : BaseTests
{
    [Theory]
    [InlineData(typeof(Container))]
    [InlineData(typeof(HttpEndpoint))]
    [InlineData(typeof(Mount))]
    public void GetContractType_ExistingType_ReturnsCorrectType(Type type)
    {
        var contractRegistry = Resolve<ContractRegistry>();

        var resultingType = contractRegistry.GetContractType(type.Name);

        Assert.Same(type, resultingType);
    }
    
    [Fact]
    public void GetContractType_NonExistingType_ThrowsException()
    {
        var contractRegistry = Resolve<ContractRegistry>();

        Assert.Throws<Exception>(() => contractRegistry.GetContractType("NonExistingType"));
    }
    
    [Theory]
    [InlineData(typeof(Container))]
    [InlineData(typeof(File))]
    [InlineData(typeof(HttpEndpoint))]
    [InlineData(typeof(Mysql))]
    [InlineData(typeof(Network))]
    [InlineData(typeof(Package))]
    [InlineData(typeof(Parameter))]
    [InlineData(typeof(Password))]
    [InlineData(typeof(Volume))]
    public void CreateContract_ExistingType_ReturnsCorrectContract(Type type)
    {
        var contractRegistry = Resolve<ContractRegistry>();
        var faker = Resolve<Faker>();
        var name = faker.Lorem.Word();

        var contract = contractRegistry.CreateContract(type.Name, name);

        Assert.IsType(type, contract);
        Assert.Equal(name, contract.Name);
    }
    
    [Theory]
    [InlineData("NonExistingType")]
    [InlineData(nameof(Mount))]
    public void CreateContract_InvalidType_ThrowsException(string invalidType)
    {
        var contractRegistry = Resolve<ContractRegistry>();
        var faker = Resolve<Faker>();
        var name = faker.Lorem.Word();

        Assert.Throws<Exception>(() => contractRegistry.CreateContract(invalidType, name));
    }
}