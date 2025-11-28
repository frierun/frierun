using Bogus;
using Frierun.Server.Data;

namespace Frierun.Tests.Data;

public class MergerTests : BaseTests
{
    [Fact]
    public void MergeDictionaries_ArgumentsWithSameKeys_MergesValues()
    {
        var value = Resolve<Faker>().Lorem.Word();
        var name = Resolve<Faker>().Lorem.Word();

        var dictionary1 = new Dictionary<string, Argument<string>>()
        {
            [name] = new(value)
        };
        var dictionary2 = new Dictionary<string, Argument<string>>()
        {
            [name] = new(value)
        };
        
        var result = Merger.MergeDictionary(dictionary1, dictionary2);
        
        Assert.Single(result);
        Assert.Equal(value, result[name].Value);
    }
    
    [Fact]
    public void MergeDictionaries_DifferentArgumentsWithSameKeys_ThrowsException()
    {
        var value = Resolve<Faker>().Lorem.Word();
        var name = Resolve<Faker>().Lorem.Word();

        var dictionary1 = new Dictionary<string, Argument<string>>()
        {
            [name] = new(value)
        };
        var dictionary2 = new Dictionary<string, Argument<string>>()
        {
            [name] = new(value + "_another")
        };
        
        Assert.Throws<MergeException>(() => Merger.MergeDictionary(dictionary1, dictionary2));
    }
    
    [Fact]
    public void OnlyOne_OneString_ReturnsString()
    {
        var value = Resolve<Faker>().Lorem.Word();
        
        Assert.Equal(value, Merger.MergeValue(value, null));
        Assert.Equal(value, Merger.MergeValue(null, value));
        Assert.Equal(value, Merger.MergeValue(value, value));
    }
    
    [Fact]
    public void OnlyOne_DifferentStrings_ThrowsException()
    {
        Assert.Throws<MergeException>(() => Merger.MergeValue("value1", "value2"));
    }

    [Fact]
    public void OnlyOne_NotZeroNumber_ReturnsNumber()
    {
        var value = Resolve<Faker>().Random.Int();
        if (value == 0)
        {
            value = 1;
        }
        
        Assert.Equal(value, Merger.MergeValue(value, 0));
        Assert.Equal(value, Merger.MergeValue(0, value));
        Assert.Equal(value, Merger.MergeValue(value, value));
    }
    
    [Fact]
    public void OnlyOne_DifferentNumbers_ThrowsException()
    {
        Assert.Throws<MergeException>(() => Merger.MergeValue(1, 2));
    }
}