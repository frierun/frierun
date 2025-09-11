using System.Text.Json;
using System.Text.Json.Serialization;
using Frierun.Server.Data;

namespace Frierun.Server;

public class ContractListConverter(ContractRegistry contractRegistry) : JsonConverter<ContractList>
{
    private delegate Contract? ReadDelegate(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    );

    /// <summary>
    /// Create a delegate for reading a contract of a given type.
    /// </summary>
    private ReadDelegate GetDelegateForType(Type contractType, JsonSerializerOptions options)
    {
        var contractConverter = options.GetConverter(contractType);
        return typeof(JsonConverter<>)
            .MakeGenericType(contractType)
            .GetMethod("Read")!
            .CreateDelegate<ReadDelegate>(contractConverter);
    }

    public override ContractList? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException();
        }

        var contractIdConverter = (JsonConverter<ContractId>)options.GetConverter(typeof(ContractId));
        var dictionary = new Dictionary<ContractId, Contract>();
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

            var contractId = contractIdConverter.ReadAsPropertyName(ref reader, typeof(ContractId), options);
            reader.Read();

            var contractType = ContractRegistry.GetContractType(contractId.TypeName);
            var contract = GetDelegateForType(contractType, options)(ref reader, contractType, options);
            dictionary[contractId] = contract ?? contractRegistry.CreateContract(contractId);
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