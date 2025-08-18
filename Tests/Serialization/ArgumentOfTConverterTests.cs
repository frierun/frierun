using System.Text.Json;
using Bogus;
using Frierun.Server;
using Frierun.Server.Data;

namespace Frierun.Tests;

public class ArgumentOfTConverterTests : BaseTests
{
    private readonly JsonSerializerOptions _options = new()
    {
        Converters =
        {
            new ArgumentOfTConverter()
        }
    };

    [Fact]
    public void Read_String_ReturnsString()
    {
        var value = Resolve<Faker>().Lorem.Word();

        var result = JsonSerializer.Deserialize<Argument<string>>(
            $"""
             "{value}"
             """,
            _options
        );

        Assert.NotNull(result);
        Assert.Equal(value, result.Value);
    }

    [Fact]
    public void Write_String_ReturnsString()
    {
        var value = Resolve<Faker>().Lorem.Word();
        var argument = new Argument<string>(value);

        var result = JsonSerializer.Serialize(argument, _options);

        Assert.Equal(
            $"""
             "{value}"
             """,
            result
        );
    }
    
    [Fact]
    public void Read_NullString_ReturnsNull()
    {
        var result = JsonSerializer.Deserialize<Argument<string>>("null", _options);

        Assert.NotNull(result);
        Assert.Null(result.Value);
    }
    
    [Fact]
    public void Write_NullString_ReturnsNull()
    {
        var argument = new Argument<string>((string?)null);

        var result = JsonSerializer.Serialize(argument, _options);

        Assert.Equal("null", result);
    }

    [Fact]
    public void Read_Int_ReturnsInt()
    {
        var value = Resolve<Faker>().Random.Int();

        var result = JsonSerializer.Deserialize<Argument<int>>(value.ToString(), _options);

        Assert.NotNull(result);
        Assert.Equal(value, result.Value);
    }
    
    [Fact]
    public void Write_Int_ReturnsInt()
    {
        var value = Resolve<Faker>().Random.Int();
        var argument = new Argument<int>(value);

        var result = JsonSerializer.Serialize(argument, _options);

        Assert.Equal(value.ToString(), result);
    }
    
    [Fact]
    public void Read_TemplateString_CreatesResolver()
    {
        var result = JsonSerializer.Deserialize<Argument<string>>("\"{{Parameter:Test:Value}}\"", _options);

        Assert.NotNull(result);
        Assert.Null(result.Value);
        Assert.NotNull(result.Resolver);
        var requiredContract = result.RequiredContracts.Single();
        Assert.Equal(new ContractId<Parameter>("Test"), requiredContract);;
    }

    [Fact]
    public void Write_TemplateString_ReturnsNull()
    {
        var argument = new Argument<string>("{{Parameter:Test:Value}}");

        var result = JsonSerializer.Serialize(argument, _options);

        Assert.Equal("null", result);
    }

    [Fact]
    public void Write_ArgumentWithResolver_ReturnsNull()
    {
        var argument = new Argument<string>(_ => "test");

        var result = JsonSerializer.Serialize(argument, _options);

        Assert.Equal("null", result);
    }
    
}