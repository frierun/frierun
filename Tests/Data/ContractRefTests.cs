using Bogus;
using Frierun.Server.Data;

namespace Frierun.Tests.Data;

public class ContractRefTests : BaseTests
{
    [Fact]
    public void Equals_SameId_ReturnsTrue()
    {
        var name = Resolve<Faker>().Lorem.Word();
        var id1 = new ContractRef<Parameter>(name);
        var id2 = new ContractRef<Parameter>(name);

        Assert.Equal(id1, id2);
        Assert.Equal(id2, id1);
    }

    [Fact]
    public void GetHashCode_SameId_ReturnsSameValue()
    {
        var name = Resolve<Faker>().Lorem.Word();
        var id1 = new ContractRef<Parameter>(name);
        var id2 = new ContractRef<Parameter>(name);

        Assert.Equal(id1.GetHashCode(), id2.GetHashCode());
    }
    
    [Fact]
    public void Equals_GenericAndNonGeneric_ReturnsTrue()
    {
        var name = Resolve<Faker>().Lorem.Word();
        var id1 = new ContractRef(nameof(Parameter), name);
        var id2 = new ContractRef<Parameter>(name);

        Assert.Equal(id1, id2);
        Assert.Equal(id2, id1);
        Assert.True(id1 == id2);
        Assert.True(id2 == id1);
    }
    
    [Fact]
    public void GetHashCode_GenericAndNonGeneric_ReturnsSameValue()
    {
        var name = Resolve<Faker>().Lorem.Word();
        var id1 = new ContractRef(nameof(Parameter), name);
        var id2 = new ContractRef<Parameter>(name);

        Assert.Equal(id1.GetHashCode(), id2.GetHashCode());
    }
}