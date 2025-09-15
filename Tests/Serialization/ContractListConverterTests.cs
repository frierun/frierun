using System.Text.Json;
using Bogus;
using Frierun.Server;
using Frierun.Server.Data;

namespace Frierun.Tests;

public class ContractListConverterTests : BaseTests
{
    private JsonSerializerOptions CreateOptions()
    {
        return new JsonSerializerOptions
        {
            Converters =
            {
                new ContractIdConverter(),
                new ContractListConverter(Resolve<ContractRegistry>())
            }
        };
    }

    [Fact]
    public void Read_EmptyObject_ReturnsEmptyContractList()
    {
        var result = JsonSerializer.Deserialize<ContractList>("{}", CreateOptions());

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void Read_ObjectWithContainerEntry_ReturnsContractList()
    {
        var containerName = Resolve<Faker>().Lorem.Word();
        var json = $$"""
                     {
                         "Container:{{containerName}}": {
                             "Name": "{{containerName}}"
                         }
                     }
                     """;

        var result = JsonSerializer.Deserialize<ContractList>(json, CreateOptions());

        Assert.NotNull(result);
        Assert.Single(result);
        var contractId = new ContractId(nameof(Container), containerName);
        Assert.True(result.ContainsKey(contractId));
        Assert.IsType<Container>(result[contractId]);
    }

    [Fact]
    public void Read_ObjectWithMultipleEntries_ReturnsContractList()
    {
        var containerName1 = Resolve<Faker>().Lorem.Word();
        var containerName2 = Resolve<Faker>().Lorem.Word();
        var json = $$"""
                     {
                         "Container:{{containerName1}}": {
                             "Name": "{{containerName1}}"
                         },
                         "Container:{{containerName2}}": {
                             "Name": "{{containerName2}}"
                         }
                     }
                     """;

        var result = JsonSerializer.Deserialize<ContractList>(json, CreateOptions());

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        var contractId1 = new ContractId(nameof(Container), containerName1);
        var contractId2 = new ContractId(nameof(Container), containerName2);
        Assert.True(result.ContainsKey(contractId1));
        Assert.True(result.ContainsKey(contractId2));
    }

    [Fact]
    public void Read_InvalidJsonToken_ThrowsJsonException()
    {
        var json = "[]";

        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<ContractList>(json, CreateOptions()));
    }

    [Fact]
    public void Read_ContainerWithoutName_ReturnsContractList()
    {
        var json = """
                   {
                       "Container": {
                           "Name": ""
                       }
                   }
                   """;

        var result = JsonSerializer.Deserialize<ContractList>(json, CreateOptions());

        Assert.NotNull(result);
        Assert.Single(result);
        var contractId = new ContractId(nameof(Container), "");
        Assert.True(result.ContainsKey(contractId));
    }

    [Fact]
    public void Write_EmptyContractList_ReturnsEmptyObject()
    {
        var json = JsonSerializer.Serialize(new ContractList(), CreateOptions());
        Assert.Equal("{}", json);
    }

    [Fact]
    public void Write_SingleContainer_ProducesExpectedJson()
    {
        var name = Resolve<Faker>().Lorem.Word();
        var contract = new Container();
        var contracts = new ContractList
        {
            [name] = contract
        };

        var json = JsonSerializer.Serialize(contracts, CreateOptions());

        var expected = $$"""
                         {"Container:{{name}}":{{JsonSerializer.Serialize<Contract>(contract, CreateOptions())}}}
                         """;
        Assert.Equal(expected, json);
    }

    [Fact]
    public void Write_MultipleContainers_ProducesExpectedJson()
    {
        var name1 = Resolve<Faker>().Lorem.Word();
        var name2 = Resolve<Faker>().Lorem.Word();
        var contract1 = new Container();
        var contract2 = new Container();
        var contracts = new ContractList
        {
            [name1] = contract1,
            [name2] = contract2
        };

        var json = JsonSerializer.Serialize(contracts, CreateOptions());

        var expected = $$"""
                         {"Container:{{name1}}":{{JsonSerializer.Serialize<Contract>(contract1, CreateOptions())}},"Container:{{name2}}":{{JsonSerializer.Serialize<Contract>(contract2, CreateOptions())}}}
                         """;

        Assert.Equal(expected, json);
    }

    [Fact]
    public void Write_ContainerWithoutName_UsesTypeOnlyKey()
    {
        var contract = new Container();
        var contracts = new ContractList
        {
            [""] = contract
        };

        var json = JsonSerializer.Serialize(contracts, CreateOptions());

        var expected = $$"""
                         {"Container:":{{JsonSerializer.Serialize<Contract>(contract, CreateOptions())}}}
                         """;
        Assert.Equal(expected, json);
    }
}