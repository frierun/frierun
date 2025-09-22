using System.Text.Json;
using Bogus;
using Frierun.Server;
using Frierun.Server.Data;

namespace Frierun.Tests;

public class ContractIdOfTConverterTests : BaseTests
{
    private JsonSerializerOptions CreateOptions()
    {
        return new JsonSerializerOptions
        {
            Converters =
            {
                new ContractRefOfTConverter(),
                new ContractIdOfTConverter()
            }
        };
    }

    [Fact]
    public void Read_Guid_ReturnsExpectedValue()
    {
        var guid = Guid.CreateVersion7();
        
        var result = JsonSerializer.Deserialize<ContractId<Container>>($"\"{guid}\"", CreateOptions());
        
        Assert.NotNull(result);
        Assert.NotNull(result.Guid);
        Assert.Null(result.Ref);
        Assert.Equal(guid, result.Guid);
    }
    
    [Fact]
    public void Read_Name_ReturnsExpectedValue()
    {
        var name = Resolve<Faker>().Lorem.Word();
        
        var result = JsonSerializer.Deserialize<ContractId<Container>>($"\"{name}\"", CreateOptions());
        
        Assert.NotNull(result);
        Assert.Null(result.Guid);
        Assert.NotNull(result.Ref);
        Assert.Equal(name, result.Ref.Name);
        Assert.Equal(nameof(Container), result.Ref.TypeName);
    }
    
    [Fact]
    public void Write_Guid_ReturnsExpectedValue()
    {
        var guid = Guid.CreateVersion7();
        var contractId = new ContractId<Container>(guid);
        
        var result = JsonSerializer.Serialize(contractId, CreateOptions());
        
        Assert.Equal($"\"{guid}\"", result);
    }
    
    [Fact]
    public void Write_Name_ReturnsExpectedValue()
    {
        var name = Resolve<Faker>().Lorem.Word();
        var contractId = new ContractId<Container>(name);
        
        var result = JsonSerializer.Serialize(contractId, CreateOptions());
        
        Assert.Equal($"\"{name}\"", result);
    }    
}