using Frierun.Server;

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
                "url": "http://ima.ge/image.png"
            }
            """u8;
        var package = packageSerializer.Load(new MemoryStream(json.ToArray()));
        Assert.NotNull(package);
        Assert.Equal("frierun", package.Name);
        Assert.Equal("http://ima.ge/image.png", package.Url);
    }
}