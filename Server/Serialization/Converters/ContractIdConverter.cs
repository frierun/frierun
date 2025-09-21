using System.Diagnostics;
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

        if (Guid.TryParse(value, out var guid))
        {
            return new ContractId(guid);
        }
        
        var contractRefConverter = (JsonConverter<ContractRef>)options.GetConverter(typeof(ContractRef));
        var contractRef = contractRefConverter.Read(ref reader, typeof(ContractRef), options);
        if (contractRef is null)
        {
            return null;
        }
        return new ContractId(contractRef);
    }

    public override void Write(Utf8JsonWriter writer, ContractId value, JsonSerializerOptions options)
    {
        if (value.Guid != null)
        {
            writer.WriteStringValue(value.Guid.Value.ToString());
            return;
        }
        
        Debug.Assert(value.Ref != null, "Contract ID must have either a GUID or a ContractRef");
        var contractRefConverter = (JsonConverter<ContractRef>)options.GetConverter(typeof(ContractRef));
        contractRefConverter.Write(writer, value.Ref, options);
    }
}