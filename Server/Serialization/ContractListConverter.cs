using System.Text.Json;
using System.Text.Json.Serialization;
using Frierun.Server.Data;

namespace Frierun.Server;

public class ContractListConverter : JsonConverter<ContractList>
{
    public override ContractList? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var converter = (JsonConverter<IEnumerable<Contract>>)options.GetConverter(typeof(IEnumerable<Contract>));
        var list = converter.Read(ref reader, typeof(List<Contract>), options);
        if (list is null)
        {
            return null;
        }

        return new ContractList(list);
    }

    public override void Write(Utf8JsonWriter writer, ContractList value, JsonSerializerOptions options)
    {
        var converter = (JsonConverter<IEnumerable<Contract>>)options.GetConverter(typeof(IEnumerable<Contract>));
        converter.Write(writer, value.Values, options);
    }
}