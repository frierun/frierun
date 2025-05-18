using Frierun.Server;
using Frierun.Server.Data;

namespace Frierun.Tests;

public class PackageSerializerTests : BaseTests
{
    [Fact]
    public void Load_SimplePackage_CreatesPackage()
    {
        var packageSerializer = Resolve<PackageSerializer>();
        var json =
            """
            {"name":"frierun"}
            """u8;
        var package = packageSerializer.Load(new MemoryStream(json.ToArray()));
        Assert.NotNull(package);
        Assert.Equal("frierun", package.Name);
    }

    [Fact]
    public void Load_PackageWithDependencies_ReadsDependencyFields()
    {
        var packageSerializer = Resolve<PackageSerializer>();
        var json =
            """
            {
                "name":"frierun",
                "dependsOn": ["Container:container1"],
                "dependencyOf": ["Container:container2"]
            }
            """u8;
        var package = packageSerializer.Load(new MemoryStream(json.ToArray()));
        Assert.NotNull(package);
        Assert.Equal("frierun", package.Name);
        Assert.Single(package.DependsOn);
        Assert.Equal(new ContractId<Container>("container1"), package.DependsOn.First());
        Assert.Single(package.DependencyOf);
        Assert.Equal(new ContractId<Container>("container2"), package.DependencyOf.First());
    }
}