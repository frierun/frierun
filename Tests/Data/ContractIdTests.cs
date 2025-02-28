using Bogus;
using Frierun.Server.Data;

namespace Frierun.Tests.Data;

public class ContractIdTests : BaseTests
{
    [Fact]
    public void Equals_SameId_ReturnsTrue()
    {
        var id1 = new ContractId<Package>("package");
        var id2 = new ContractId<Package>("package");

        Assert.Equal(id1, id2);
        Assert.Equal(id2, id1);
    }

    [Fact]
    public void GetHashCode_SameId_ReturnsSameValue()
    {
        var id1 = new ContractId<Package>("package");
        var id2 = new ContractId<Package>("package");

        Assert.Equal(id1.GetHashCode(), id2.GetHashCode());
    }
    
    [Fact]
    public void Equals_GenericAndNonGeneric_ReturnsTrue()
    {
        var id1 = ContractId.Create(typeof(Package), "package");
        var id2 = new ContractId<Package>("package");

        Assert.Equal(id1, id2);
        Assert.Equal(id2, id1);
    }
    
    [Fact]
    public void GetHashCode_GenericAndNonGeneric_ReturnsSameValue()
    {
        var id1 = ContractId.Create(typeof(Package), "package");
        var id2 = new ContractId<Package>("package");

        Assert.Equal(id1.GetHashCode(), id2.GetHashCode());
    }
}