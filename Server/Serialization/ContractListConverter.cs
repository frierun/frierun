using System.Text.Json;
using System.Text.Json.Serialization;
using Frierun.Server.Data;

namespace Frierun.Server;

public class ContractListConverter : JsonConverter<ContractList>
{
    public override ContractList? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.StartArray)
        {
            var listConverter = (JsonConverter<IEnumerable<Contract>>)
                options.GetConverter(typeof(IEnumerable<Contract>));
            var list = listConverter.Read(ref reader, typeof(IEnumerable<Contract>), options);
            if (list is null)
            {
                return null;
            }

            return new ContractList(
                list.Select(contract => new KeyValuePair<ContractId, Contract>(contract.Id, contract))
            );
        }

        var dictionaryConverter = (JsonConverter<IReadOnlyDictionary<ContractId, Contract>>)
            options.GetConverter(typeof(IReadOnlyDictionary<ContractId, Contract>));
        var dictionary = dictionaryConverter.Read(
            ref reader, typeof(IReadOnlyDictionary<ContractId, Contract>), options
        );
        if (dictionary is null)
        {
            return null;
        }

        return new ContractList(dictionary);
    }

    public override void Write(Utf8JsonWriter writer, ContractList value, JsonSerializerOptions options)
    {
        var converter = (JsonConverter<IReadOnlyDictionary<ContractId, Contract>>)
            options.GetConverter(typeof(IReadOnlyDictionary<ContractId, Contract>));
        converter.Write(writer, value, options);
    }
}