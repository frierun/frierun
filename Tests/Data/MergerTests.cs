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
        
        var result = Merger.MergeDictionaries(dictionary1, dictionary2);
        
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
        
        Assert.Throws<MergeException>(() => Merger.MergeDictionaries(dictionary1, dictionary2));
    }    
}