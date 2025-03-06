using System.Text;
using System.Text.Json;
using Frierun.Server;
using Frierun.Server.Data;
using Frierun.Server.Services;

namespace Frierun.Tests.Services;

public class PackageConverterTests : BaseTests
{
    [Fact]
    public void Read_Frierun_ReturnsPackage()
    {
        var packageRegistry = Resolve<PackageRegistry>();
        packageRegistry.Load();
        var converter = new PackageConverter(packageRegistry);
        var reader = new Utf8JsonReader("\"frierun\""u8);
        reader.Read();
        var options = new JsonSerializerOptions();

        var result = converter.Read(ref reader, typeof(Package), options);

        Assert.NotNull(result);
        Assert.Equal(packageRegistry.Find("frierun"), result);
    }

    [Fact]
    public void Read_NonExistentPackage_ReturnsNull()
    {
        var packageRegistry = Resolve<PackageRegistry>();
        var converter = new PackageConverter(packageRegistry);
        var reader = new Utf8JsonReader("\"frierun\""u8);
        reader.Read();
        var options = new JsonSerializerOptions();

        var result = converter.Read(ref reader, typeof(Package), options);

        Assert.Null(result);
    }

    [Fact]
    public void Write_FrierunPackage_ReturnsName()
    {
        var packageRegistry = Resolve<PackageRegistry>();
        packageRegistry.Load();
        var package = packageRegistry.Find("frierun");
        Assert.NotNull(package);
        var converter = new PackageConverter(packageRegistry);
        var memoryStream = new MemoryStream();
        var writer = new Utf8JsonWriter(memoryStream);
        var options = new JsonSerializerOptions();

        converter.Write(writer, package, options);
        writer.Flush();
        var result = Encoding.UTF8.GetString(memoryStream.ToArray());

        Assert.Equal("\"frierun\"", result);
    }
}