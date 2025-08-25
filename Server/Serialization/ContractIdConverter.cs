using System.Text.Json;
using System.Text.Json.Serialization;
using Frierun.Server.Data;

namespace Frierun.Server;

public class ContractIdConverter : JsonConverter<ContractId>
{
    public override ContractId? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        if (value is null)
        {
            return null;
        }
        
        var parts = value.Split(':', 2);
        return new ContractId(parts[0], parts.Length == 2 ?  parts[1] : "");
    }

    public override void Write(Utf8JsonWriter writer, ContractId value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}