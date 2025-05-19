using System.Text.Json;
using Bogus;
using Frierun.Server;
using Frierun.Server.Data;

namespace Frierun.Tests;

public class ContractIdOfTConverterTests : BaseTests
{
    [Fact]
    public void Read_Container_ReturnsContainer()
    {
        var converter = new ContractIdOfTConverter();
        var name = Resolve<Faker>().Lorem.Word();

        var result = JsonSerializer.Deserialize<ContractId<Container>>(
            $"""
             "{name}"
             """,
            new JsonSerializerOptions
            {
                Converters =
                {
                    converter
                }
            }
        );

        Assert.NotNull(result);
        Assert.IsType<ContractId<Container>>(result);
    }

    [Fact]
    public void Write_Container_ReturnsName()
    {
        var converter = new ContractIdOfTConverter();
        var name = Resolve<Faker>().Lorem.Word();
        var container = new ContractId<Container>(name);

        var result = JsonSerializer.Serialize(
            container, new JsonSerializerOptions
            {
                Converters =
                {
                    converter
                }
            }
        );

        Assert.Equal(
            $"""
             "{name}"
             """,
            result
        );
    }
}