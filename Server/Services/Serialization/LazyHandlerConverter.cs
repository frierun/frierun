using System.Text.Json;
using System.Text.Json.Serialization;
using Frierun.Server.Installers;

namespace Frierun.Server;

public class LazyHandlerConverter(Lazy<InstallerRegistry> lazyInstallerRegistry) : JsonConverter<Lazy<IHandler>>
{
    /// <inheritdoc />
    public override Lazy<IHandler> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string typeName = string.Empty;
        string? applicationName = null;

        if (reader.TokenType == JsonTokenType.Null)
        {
            throw new JsonException();
        }

        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException();
        }

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                break;
            }

            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException();
            }

            string? propertyName = reader.GetString();
            reader.Read();
            switch (propertyName)
            {
                case "TypeName":
                    typeName = reader.GetString() ?? throw new JsonException();
                    break;
                case "ApplicationName":
                    applicationName = reader.GetString();
                    break;
                default:
                    throw new JsonException();
            }
        }

        if (typeName == string.Empty)
        {
            throw new JsonException();
        }

        return new Lazy<IHandler>(
            () => lazyInstallerRegistry.Value.GetHandler(typeName, applicationName)
                  ?? throw new Exception($"Handler with type name {typeName} not found")
        );
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, Lazy<IHandler> lazy, JsonSerializerOptions options)
    {
        var value = lazy.Value;

        writer.WriteStartObject();
        writer.WritePropertyName("TypeName");
        writer.WriteStringValue(value.GetType().Name);
        if (value.Application != null)
        {
            writer.WritePropertyName("ApplicationName");
            writer.WriteStringValue(value.Application.Name);
        }

        writer.WriteEndObject();
    }
}