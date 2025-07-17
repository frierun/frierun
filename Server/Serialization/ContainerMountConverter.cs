using System.Text.Json.Serialization;
using Frierun.Server.Data;
using System.Text.Json;

namespace Frierun.Server;

public class ContainerMountConverter : JsonConverter<ContainerMount>
{
    public override ContainerMount? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var volumeName = reader.GetString();
            return new ContainerMount(Volume: new ContractId<Volume>(volumeName ?? ""));
        }

        var newOptions = new JsonSerializerOptions(options);
        foreach (var converter in newOptions.Converters)
        {
            if (converter is ContainerMountConverter)
            {
                newOptions.Converters.Remove(converter);
                break;
            }
        }
        return JsonSerializer.Deserialize<ContainerMount>(ref reader, newOptions);
    }

    public override void Write(Utf8JsonWriter writer, ContainerMount value, JsonSerializerOptions options)
    {
        var newOptions = new JsonSerializerOptions(options);
        foreach (var converter in newOptions.Converters)
        {
            if (converter is ContainerMountConverter)
            {
                newOptions.Converters.Remove(converter);
                break;
            }
        }
        JsonSerializer.Serialize(writer, value, newOptions);
    }
}