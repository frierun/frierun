using Bogus;
using Frierun.Server.Data;

namespace Frierun.Tests.Data;

public class ContractRefTests : BaseTests
{
    [Fact]
    public void Equals_SameId_ReturnsTrue()
    {
        var name = Resolve<Faker>().Lorem.Word();
        var ref1 = new ContractRef<Parameter>(name);
        var ref2 = new ContractRef<Parameter>(name);

        Assert.Equal(ref1, ref2);
        Assert.Equal(ref2, ref1);
    }

    [Fact]
    public void GetHashCode_SameId_ReturnsSameValue()
    {
        var name = Resolve<Faker>().Lorem.Word();
        var ref1 = new ContractRef<Parameter>(name);
        var ref2 = new ContractRef<Parameter>(name);

        Assert.Equal(ref1.GetHashCode(), ref2.GetHashCode());
    }
    
    [Fact]
    public void Equals_GenericAndNonGeneric_ReturnsTrue()
    {
        var name = Resolve<Faker>().Lorem.Word();
        var ref1 = new ContractRef(nameof(Parameter), name);
        var ref2 = new ContractRef<Parameter>(name);

        Assert.Equal(ref1, ref2);
        Assert.Equal(ref2, ref1);
        Assert.True(ref1 == ref2);
        Assert.True(ref2 == ref1);
    }
    
    [Fact]
    public void GetHashCode_GenericAndNonGeneric_ReturnsSameValue()
    {
        var name = Resolve<Faker>().Lorem.Word();
        var ref1 = new ContractRef(nameof(Parameter), name);
        var ref2 = new ContractRef<Parameter>(name);

        Assert.Equal(ref1.GetHashCode(), ref2.GetHashCode());
    }
}