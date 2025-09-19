using System.Text;
using System.Text.Json;
using Bogus;
using Frierun.Server;
using Frierun.Server.Data;

namespace Frierun.Tests;

public class ContractRefConverterTests : BaseTests
{
    [Fact]
    public void Read_ContainerWithName_ReturnsExpectedValue()
    {
        var converter = new ContractRefConverter();
        var name = Resolve<Faker>().Lorem.Word();
        var reader = new Utf8JsonReader(
            Encoding.UTF8.GetBytes(
                $"""
                 "Container:{name}"
                 """
            )
        );
        reader.Read();
        var options = new JsonSerializerOptions();

        var result = converter.Read(ref reader, typeof(ContractRef), options);

        Assert.NotNull(result);
        Assert.Equal(nameof(Container), result.TypeName);
        Assert.Equal(name, result.Name);
    }

    [Fact]
    public void Read_ContainerWithoutName_ReturnsExpectedValue()
    {
        var converter = new ContractRefConverter();
        var reader = new Utf8JsonReader(
            """
            "Container"
            """u8
        );
        reader.Read();
        var options = new JsonSerializerOptions();

        var result = converter.Read(ref reader, typeof(ContractRef), options);

        Assert.NotNull(result);
        Assert.Equal(nameof(Container), result.TypeName);
        Assert.Equal("", result.Name);
    }
}