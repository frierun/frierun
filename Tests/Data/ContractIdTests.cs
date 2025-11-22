using Frierun.Server.Data;

namespace Frierun.Tests.Data;

public class ContractIdTests : BaseTests
{
    public static IEnumerable<object[]> SameIds()
    {
        var guid = Guid.CreateVersion7();
        yield return [new ContractId<Parameter>(guid), new ContractId<Parameter>(guid)];
        yield return [new ContractId<Selector>(guid), new ContractId<Parameter>(guid)];
        yield return [new ContractId(guid), new ContractId<Parameter>(guid)];

        yield return
        [
            new ContractId<Parameter>(Guid.Empty, "test_name"),
            new ContractId<Parameter>(Guid.Empty, "test_name")
        ];

        yield return
        [
            new ContractId(Guid.Empty, new ContractRef<Parameter>("test_name")),
            new ContractId<Parameter>(Guid.Empty, "test_name")
        ];
        
        yield return [new ContractId<Parameter>(guid), new ContractId<Parameter>(guid, "test_name")];
        yield return [new ContractId<Selector>(guid), new ContractId<Parameter>(guid, "test_name")];
        yield return [new ContractId(guid), new ContractId<Parameter>(guid, "test_name")];
    }

    public static IEnumerable<object[]> DifferentIds()
    {
        yield return
        [
            new ContractId<Selector>(Guid.Empty, "test_name"),
            new ContractId<Parameter>(Guid.Empty, "test_name")
        ];

        yield return
        [
            new ContractId<Parameter>(Guid.CreateVersion7()),
            new ContractId<Parameter>(Guid.CreateVersion7())
        ];
        yield return [new ContractId<Parameter>(Guid.CreateVersion7()), new ContractId<Parameter>(Guid.Empty)];

        yield return [new ContractId<Parameter>(Guid.Empty, "test_name"), new ContractId<Parameter>(Guid.Empty)];
    }

    [Theory]
    [MemberData(nameof(SameIds))]
    public void Equals_SameIds_ReturnsTrue(ContractId id1, ContractId id2)
    {
        Assert.Equal(id1, id2);
        Assert.Equal(id2, id1);
        Assert.True(id1 == id2);
        Assert.True(id2 == id1);
    }

    [Theory]
    [MemberData(nameof(SameIds))]
    public void GetHashCode_SameIds_ReturnsSameValue(ContractId id1, ContractId id2)
    {
        Assert.Equal(id1.GetHashCode(), id2.GetHashCode());
    }

    [Theory]
    [MemberData(nameof(DifferentIds))]
    public void Equals_DifferentIds_ReturnsFalse(ContractId id1, ContractId id2)
    {
        Assert.NotEqual(id1, id2);
        Assert.NotEqual(id2, id1);
        Assert.True(id1 != id2);
        Assert.True(id2 != id1);
    }
    
    [Theory]
    [MemberData(nameof(DifferentIds))]
    public void GetHashCode_DifferentIds_ReturnsDifferentValue(ContractId id1, ContractId id2)
    {
        Assert.NotEqual(id1.GetHashCode(), id2.GetHashCode());
    }
    
}