using System.Text.Json;
using System.Text.Json.Serialization;
using Frierun.Server.Handlers;

namespace Frierun.Server;

public class LazyHandlerConverter(Lazy<HandlerRegistry> lazyHandlerRegistry) : JsonConverter<Lazy<IHandler?>>
{
    public override Lazy<IHandler?> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string typeName = string.Empty;
        string? applicationName = null;

        if (reader.TokenType == JsonTokenType.Null)
        {
            return new Lazy<IHandler?>((IHandler?)null);
        }

        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException();
        }

        string Convert(string name) => options.PropertyNamingPolicy?.ConvertName(name) ?? name;
        
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
            if (propertyName == Convert("TypeName"))
            {
                typeName = reader.GetString() ?? throw new JsonException();
                continue;
            }
            if (propertyName == Convert("ApplicationName"))
            {
                applicationName = reader.GetString();
                continue;
            }
                    
            throw new JsonException();
        }

        if (typeName == string.Empty)
        {
            throw new JsonException();
        }

        return new Lazy<IHandler?>(
            () => lazyHandlerRegistry.Value.GetHandler(typeName, applicationName)
        );
    }

    public override void Write(Utf8JsonWriter writer, Lazy<IHandler?> lazy, JsonSerializerOptions options)
    {
        var value = lazy.Value;

        if (value == null)
        {
            writer.WriteNullValue();
            return;
        }

        string Convert(string name) => options.PropertyNamingPolicy?.ConvertName(name) ?? name;
        writer.WriteStartObject();
        writer.WritePropertyName(Convert("TypeName"));
        writer.WriteStringValue(value.GetType().Name);
        if (value.Application != null)
        {
            writer.WritePropertyName(Convert("ApplicationName"));
            writer.WriteStringValue(value.Application.Name);
        }

        writer.WriteEndObject();
    }
}