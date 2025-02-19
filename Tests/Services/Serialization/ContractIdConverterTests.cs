using System.Text;
using System.Text.Json;
using Bogus;
using Frierun.Server.Data;
using Frierun.Server.Services;

namespace Frierun.Tests.Services;

public class ContractIdConverterTests : BaseTests
{
    [Fact]
    public void Read_ContainerWithName_ReturnsContractId()
    {
        var contractRegistry = Resolve<ContractRegistry>();
        var converter = new ContractIdConverter(contractRegistry);
        var name = Resolve<Faker>().Lorem.Word();
        var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes($"\"Container:{name}\""));
        reader.Read();
        var options = new JsonSerializerOptions();

        var result = converter.Read(ref reader, typeof(ContractId), options);

        Assert.NotNull(result);
        Assert.Equal(typeof(Container), result.Type);
        Assert.Equal(name, result.Name);
    }
    
    [Fact]
    public void Read_ContainerWithoutName_ReturnsContractId()
    {
        var contractRegistry = Resolve<ContractRegistry>();
        var converter = new ContractIdConverter(contractRegistry);
        var reader = new Utf8JsonReader("\"Container\""u8);
        reader.Read();
        var options = new JsonSerializerOptions();

        var result = converter.Read(ref reader, typeof(ContractId), options);

        Assert.NotNull(result);
        Assert.Equal(typeof(Container), result.Type);
        Assert.Equal("", result.Name);
    }    
}