using System.Text.Json;
using Bogus;
using Frierun.Server;
using Frierun.Server.Data;

namespace Frierun.Tests;

public class ContractIdConverterTests : BaseTests
{
    private JsonSerializerOptions CreateOptions()
    {
        return new JsonSerializerOptions
        {
            Converters =
            {
                new ContractRefConverter(),
                new ContractIdConverter()
            }
        };
    }

    [Fact]
    public void Read_Guid_ReturnsExpectedValue()
    {
        var guid = Guid.CreateVersion7();
        
        var result = JsonSerializer.Deserialize<ContractId>($"\"{guid}\"", CreateOptions());
        
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Guid);
        Assert.Null(result.Ref);
        Assert.Equal(guid, result.Guid);
    }
    
    [Fact]
    public void Read_ContainerWithName_ReturnsExpectedValue()
    {
        var name = Resolve<Faker>().Lorem.Word();

        var result = JsonSerializer.Deserialize<ContractId>($"\"{nameof(Container)}:{name}\"", CreateOptions());

        Assert.NotNull(result);
        Assert.Equal(Guid.Empty, result.Guid);
        Assert.NotNull(result.Ref);
        Assert.Equal(nameof(Container), result.Ref.TypeName);
        Assert.Equal(name, result.Ref.Name);
    }

    [Fact]
    public void Read_ContainerWithoutName_ReturnsExpectedValue()
    {
        var result = JsonSerializer.Deserialize<ContractId>($"\"{nameof(Container)}\"", CreateOptions());

        Assert.NotNull(result);
        Assert.Equal(Guid.Empty, result.Guid);
        Assert.NotNull(result.Ref);
        Assert.Equal(nameof(Container), result.Ref.TypeName);
        Assert.Equal("", result.Ref.Name);
    }
    
    [Fact]
    public void Write_Guid_ReturnsExpectedValue()
    {
        var guid = Guid.CreateVersion7();
        var contractId = new ContractId(guid);
        
        var result = JsonSerializer.Serialize(contractId, CreateOptions());
        
        Assert.Equal($"\"{guid}\"", result);
    }
    
    [Fact]
    public void Write_ContainerWithName_ReturnsExpectedValue()
    {
        var name = Resolve<Faker>().Lorem.Word();
        var contractId = new ContractId(Guid.Empty, new ContractRef(nameof(Container), name));
        
        var result = JsonSerializer.Serialize(contractId, CreateOptions());
        
        Assert.Equal($"\"{nameof(Container)}:{name}\"", result);
    }

    [Fact]
    public void Write_ContainerWithoutName_ReturnsExpectedValue()
    {
        var contractId = new ContractId(Guid.Empty, new ContractRef(nameof(Container), ""));
        
        var result = JsonSerializer.Serialize(contractId, CreateOptions());
        
        Assert.Equal($"\"{nameof(Container)}:\"", result);
    }
    
}