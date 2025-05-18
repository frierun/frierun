using System.Text;
using System.Text.Json;
using Frierun.Server;
using Frierun.Server.Handlers;
using Frierun.Server.Handlers.Base;
using Frierun.Server.Handlers.Docker;

namespace Frierun.Tests;

public class LazyHandlerConverterTests : BaseTests
{
    [Fact]
    public void Read_StaticHandler_ReturnsHandler()
    {
        var converter = new LazyHandlerConverter(Resolve<Lazy<HandlerRegistry>>());
        var typeName = nameof(PackageHandler);
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
        Assert.IsType<PackageHandler>(result.Value);
    }

    [Fact]
    public void Read_WrongType_ReturnsLazyNull()
    {
        var converter = new LazyHandlerConverter(Resolve<Lazy<HandlerRegistry>>());
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
        var converter = new LazyHandlerConverter(Resolve<Lazy<HandlerRegistry>>());
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
        var type = typeof(ContainerHandler);
        var converter = new LazyHandlerConverter(Resolve<Lazy<HandlerRegistry>>());
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
        const string typeName = nameof(PackageHandler);
        var handlerRegistry = Resolve<HandlerRegistry>();
        var handler = handlerRegistry.GetHandler(typeName);
        Assert.NotNull(handler);
        var converter = new LazyHandlerConverter(Resolve<Lazy<HandlerRegistry>>());
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
        const string typeName = nameof(ContainerHandler);
        var application = InstallPackage("docker");
        var handlerRegistry = Resolve<HandlerRegistry>();
        var handler = handlerRegistry.GetHandler(typeName, application.Name);
        Assert.NotNull(handler);
        var converter = new LazyHandlerConverter(Resolve<Lazy<HandlerRegistry>>());
        var memoryStream = new MemoryStream();
        var writer = new Utf8JsonWriter(memoryStream);
        var options = new JsonSerializerOptions();

        converter.Write(writer, new Lazy<IHandler?>(handler), options);

        writer.Flush();
        var result = Encoding.UTF8.GetString(memoryStream.ToArray());

        Assert.Equal(
            $$"""
              {"TypeName":"{{typeName}}","ApplicationName":"{{application.Name}}"}
              """,
            result
        );
    }
    
    [Fact]
    public void Write_Null_WritesNull()
    {
        var converter = new LazyHandlerConverter(Resolve<Lazy<HandlerRegistry>>());
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