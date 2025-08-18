using Bogus;
using Frierun.Server.Data;

namespace Frierun.Tests.Data;

public class ArgumentTests : BaseTests
{
    [Fact]
    public void Merge_SameResolvedValues_ReturnsValue()
    {
        var value = Resolve<Faker>().Lorem.Word();
        var arg1 = new Argument<string>(value);
        var arg2 = new Argument<string>(value);

        var result = arg1.Merge(arg2);

        Assert.Equal(value, result.Value);
    }

    [Fact]
    public void Merge_DifferentResolvedValues_ThrowsException()
    {
        var value = Resolve<Faker>().Lorem.Word();
        var arg1 = new Argument<string>(value);
        var arg2 = new Argument<string>(value + "_another");

        Assert.Throws<MergeException>(() => arg1.Merge(arg2));
    }

    [Fact]
    public void Merge_ResolverAndEmptyValue_ReturnsResolver()
    {
        var value = Resolve<Faker>().Lorem.Word();
        var arg1 = new Argument<string>(_ => value);
        var arg2 = new Argument<string>();
        
        var result = arg1.Merge(arg2);
        
        Assert.Null(result.Value);
        Assert.Equal(arg1.Resolver, result.Resolver);
    }
    
    [Fact]
    public void Merge_ResolverAndValue_ThrowsException()
    {
        var value = Resolve<Faker>().Lorem.Word();
        var arg1 = new Argument<string>(_ => value);
        var arg2 = new Argument<string>(value);
        
        Assert.Throws<MergeException>(() => arg1.Merge(arg2));
    }

    [Fact]
    public void Merge_WithRequiredContracts_CopiesContracts()
    {
        var name = Resolve<Faker>().Lorem.Word();
        var arg1 = new Argument<string>();
        var arg2 = new Argument<string>($"{{{{Parameter:{name}:Value}}}}");
        
        var result = arg1.Merge(arg2);
        
        Assert.Single(result.RequiredContracts);
        Assert.Equal(arg2.RequiredContracts, result.RequiredContracts);
    }
}