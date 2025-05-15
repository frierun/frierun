using System.Text;
using System.Text.Json;
using Frierun.Server;
using Frierun.Server.Installers;
using Frierun.Server.Installers.Base;
using Frierun.Server.Installers.Docker;

namespace Frierun.Tests;

public class LazyHandlerConverterTests : BaseTests
{
    [Fact]
    public void Read_StaticHandler_ReturnsHandler()
    {
        var converter = new LazyHandlerConverter(Resolve<Lazy<InstallerRegistry>>());
        var typeName = nameof(PackageInstaller);
        var reader = new Utf8JsonReader(
            Encoding.UTF8.GetBytes(
                $$"""
                  {"TypeName":"{{typeName}}"}
                  """
            )
        );
        reader.Read();

        var result = converter.Read(ref reader, typeof(IHandler), new JsonSerializerOptions());

        Assert.NotNull(result.Value);
        Assert.IsType<PackageInstaller>(result.Value);
    }

    [Fact]
    public void Read_WrongType_ReturnsLazyNull()
    {
        var converter = new LazyHandlerConverter(Resolve<Lazy<InstallerRegistry>>());
        var typeName = "WrongType";
        var reader = new Utf8JsonReader(
            Encoding.UTF8.GetBytes(
                $$"""
                  {"TypeName":"{{typeName}}"}
                  """
            )
        );
        reader.Read();

        var result = converter.Read(ref reader, typeof(IHandler), new JsonSerializerOptions());

        Assert.Null(result.Value);
    }

    [Fact]
    public void Read_Null_ReturnsLazyNull()
    {
        var converter = new LazyHandlerConverter(Resolve<Lazy<InstallerRegistry>>());
        var reader = new Utf8JsonReader(
            Encoding.UTF8.GetBytes(
                $$"""
                  null
                  """
            )
        );
        reader.Read();

        var result = converter.Read(ref reader, typeof(IHandler), new JsonSerializerOptions());

        Assert.Null(result.Value);
    }
    
    [Fact]
    public void Read_InstalledApplication_ReturnsHandler()
    {
        var application = InstallPackage("docker");
        var type = typeof(ContainerInstaller);
        var converter = new LazyHandlerConverter(Resolve<Lazy<InstallerRegistry>>());
        var reader = new Utf8JsonReader(
            Encoding.UTF8.GetBytes(
                $$"""
                  {"TypeName":"{{type.Name}}", "ApplicationName":"{{application.Name}}"}
                  """
            )
        );
        reader.Read();

        var result = converter.Read(ref reader, typeof(IHandler), new JsonSerializerOptions());

        Assert.NotNull(result.Value);
        Assert.IsType(type, result.Value);
    }
    
    [Fact]
    public void Write_StaticHandler_WritesNullApplicationName()
    {
        var installerRegistry = Resolve<InstallerRegistry>();
        var typeName = nameof(PackageInstaller);
        var handler = installerRegistry.GetHandler(typeName);
        Assert.NotNull(handler);
        var converter = new LazyHandlerConverter(Resolve<Lazy<InstallerRegistry>>());
        var memoryStream = new MemoryStream();
        var writer = new Utf8JsonWriter(memoryStream);
        var options = new JsonSerializerOptions();

        converter.Write(writer, new Lazy<IHandler?>(handler), options);

        writer.Flush();
        var result = Encoding.UTF8.GetString(memoryStream.ToArray());

        Assert.Equal(
            $$"""
              {"TypeName":"{{typeName}}"}
              """,
            result
        );
    }
    
    [Fact]
    public void Write_InstalledApplication_WritesApplicationName()
    {
        var application = InstallPackage("docker");
        var installerRegistry = Resolve<InstallerRegistry>();
        var type = typeof(ContainerInstaller);
        var handler = installerRegistry.GetHandler(type.Name, application.Name);
        Assert.NotNull(handler);
        var converter = new LazyHandlerConverter(Resolve<Lazy<InstallerRegistry>>());
        var memoryStream = new MemoryStream();
        var writer = new Utf8JsonWriter(memoryStream);
        var options = new JsonSerializerOptions();

        converter.Write(writer, new Lazy<IHandler?>(handler), options);

        writer.Flush();
        var result = Encoding.UTF8.GetString(memoryStream.ToArray());

        Assert.Equal(
            $$"""
              {"TypeName":"{{type.Name}}","ApplicationName":"{{application.Name}}"}
              """,
            result
        );
    }
    
    [Fact]
    public void Write_Null_WritesNull()
    {
        var converter = new LazyHandlerConverter(Resolve<Lazy<InstallerRegistry>>());
        var memoryStream = new MemoryStream();
        var writer = new Utf8JsonWriter(memoryStream);
        var options = new JsonSerializerOptions();

        converter.Write(writer, new Lazy<IHandler?>((IHandler?)null), options);

        writer.Flush();
        var result = Encoding.UTF8.GetString(memoryStream.ToArray());

        Assert.Equal(
            $$"""
              null
              """,
            result
        );
    }
    
}