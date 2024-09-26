using Frierun.Server.Models;
using Newtonsoft.Json;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

namespace Frierun.Server.Services.Serialization;

public class PackageConverter(PackageRegistry packageRegistry) : JsonConverter<Package>
{
    /// <inheritdoc />
    public override void WriteJson(JsonWriter writer, Package? value, JsonSerializer serializer)
    {
        if (value == null)
        {
            writer.WriteNull();
            return;
        }
        
        writer.WriteValue(value.Name);
    }

    /// <inheritdoc />
    public override Package? ReadJson(JsonReader reader, Type objectType, Package? existingValue, bool hasExistingValue,
        JsonSerializer serializer)
    {
        if (reader.TokenType != JsonToken.String || !(reader.Value is string name))
        {
            return null;
        }
        
        return packageRegistry.Find(name);
    }
}