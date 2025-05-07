using System.Text;
using System.Text.Json;
using Frierun.Server;
using Frierun.Server.Installers;
using Frierun.Server.Installers.Base;

namespace Frierun.Tests.Services;

public class LazyHandlerConverterTests : BaseTests
{
    [Fact]
    public void Read_StaticHandler_ReturnsHandler()
    {
        var converter = new LazyHandlerConverter(Resolve<Lazy<InstallerRegistry>>());
        var typeName = nameof(EmptyHandler);
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
        Assert.IsType<EmptyHandler>(result.Value);
    }

    [Fact]
    public void Read_WrongType_ThrowsError()
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

        Assert.Throws<Exception>(() => result.Value);
    }
    
    
    [Fact]
    public void Read_InstalledApplication_ReturnsHandler()
    {
        var application = InstallPackage("mysql");
        Assert.NotNull(application);
        var typeName = nameof(MysqlInstaller);
        var converter = new LazyHandlerConverter(Resolve<Lazy<InstallerRegistry>>());
        var reader = new Utf8JsonReader(
            Encoding.UTF8.GetBytes(
                $$"""
                  {"TypeName":"{{typeName}}", "ApplicationName":"{{application.Name}}"}
                  """
            )
        );
        reader.Read();

        var result = converter.Read(ref reader, typeof(IHandler), new JsonSerializerOptions());

        Assert.NotNull(result.Value);
        Assert.IsType<MysqlInstaller>(result.Value);
    }
    
    [Fact]
    public void Write_StaticHandler_WritesNullApplicationName()
    {
        var installerRegistry = Resolve<InstallerRegistry>();
        var typeName = nameof(EmptyHandler);
        var handler = installerRegistry.GetHandler(typeName);
        Assert.NotNull(handler);
        var converter = new LazyHandlerConverter(Resolve<Lazy<InstallerRegistry>>());
        var memoryStream = new MemoryStream();
        var writer = new Utf8JsonWriter(memoryStream);
        var options = new JsonSerializerOptions();

        converter.Write(writer, new Lazy<IHandler>(handler), options);

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
        var application = InstallPackage("mysql");
        Assert.NotNull(application);
        var installerRegistry = Resolve<InstallerRegistry>();
        var typeName = nameof(MysqlInstaller);
        var handler = installerRegistry.GetHandler(typeName, application.Name);
        Assert.NotNull(handler);
        var converter = new LazyHandlerConverter(Resolve<Lazy<InstallerRegistry>>());
        var memoryStream = new MemoryStream();
        var writer = new Utf8JsonWriter(memoryStream);
        var options = new JsonSerializerOptions();

        converter.Write(writer, new Lazy<IHandler>(handler), options);

        writer.Flush();
        var result = Encoding.UTF8.GetString(memoryStream.ToArray());

        Assert.Equal(
            $$"""
              {"TypeName":"{{typeName}}","ApplicationName":"{{application.Name}}"}
              """,
            result
        );
    }    
}