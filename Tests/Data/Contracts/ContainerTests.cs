using Frierun.Server.Data;

namespace Frierun.Tests.Data.Contracts;

public class ContainerTests : BaseTests
{
    [Fact]
    public void Merge_WithEmptyContract_CopiesFields()
    {
        var container = Factory<Container>().Generate();

        var result = (Container)new Container(Name: container.Name).Merge(container);

        Assert.Equal(container.Name, result.Name);
        Assert.Equal(container.ImageName, result.ImageName);
        Assert.Equal(container.NetworkName, result.NetworkName);
        Assert.Equal(container.ContainerName, result.ContainerName);
        Assert.Equal(container.Command, result.Command);
        Assert.Equal(container.Network, result.Network);
    }

    [Fact]
    public void Merge_TwoEmptyContracts_LeavesContractEmpty()
    {
        var container = new Container();
        var container2 = new Container();

        var result = (Container)container.Merge(container2);
        Assert.Empty(result.Name);
        Assert.Null(result.ImageName);
        Assert.Null(result.NetworkName);
        Assert.Null(result.ContainerName);
        Assert.Empty(result.Command);
        Assert.Empty(result.Env);
        Assert.Empty(result.Labels);
        Assert.Empty(result.Mounts);
    }

    [Fact]
    public void Merge_ContractsWithDifferentNames_ThrowsException()
    {
        var container = Factory<Container>().Generate();
        var container2 = container with { Name = container.Name + "2" };

        Assert.Throws<MergeException>(() => container.Merge(container2));
    }

    [Fact]
    public void Merge_ContainerWithDifferentContractType_ThrowsException()
    {
        var container = Factory<Container>().Generate() with { Name = "" };
        var volume = Factory<Volume>().Generate() with { Name = "" };

        Assert.Throws<MergeException>(() => container.Merge(volume));
    }

    [Fact]
    public void Merge_SameContractsWithDifferentEnvs_EnvsMerged()
    {
        var container = Factory<Container>().Generate() with
        {
            Env = new Dictionary<string, string> { { "key1", "value1" } }
        };
        var container2 = container with { Env = new Dictionary<string, string> { { "key2", "value2" } } };

        var result = (Container)container.Merge(container2);

        Assert.Equal(2, result.Env.Count);
        Assert.Equal("value1", result.Env["key1"]);
        Assert.Equal("value2", result.Env["key2"]);
    }

    [Fact]
    public void Merge_SameContractsWithSameEnvs_NoErrorThrown()
    {
        var container = Factory<Container>().Generate() with
        {
            Env = new Dictionary<string, string> { { "key1", "value1" } }
        };
        var container2 = container with { Env = new Dictionary<string, string> { { "key1", "value1" } } };

        var result = (Container)container.Merge(container2);

        Assert.Single(result.Env);
        Assert.Equal("value1", result.Env["key1"]);
    }

    [Fact]
    public void Merge_SameContractsWithDifferentValuesForSameEnvs_ErrorThrown()
    {
        var container = Factory<Container>().Generate() with
        {
            Env = new Dictionary<string, string> { { "key1", "value1" } }
        };
        var container2 = container with { Env = new Dictionary<string, string> { { "key1", "value2" } } };

        Assert.Throws<MergeException>(() => container.Merge(container2));
    }
}