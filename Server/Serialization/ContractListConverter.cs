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

        var contractRefConverter = (JsonConverter<ContractRef>)options.GetConverter(typeof(ContractRef));
        var dictionary = new Dictionary<ContractRef, Contract>();
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

            var contractRef = contractRefConverter.ReadAsPropertyName(ref reader, typeof(ContractRef), options);
            reader.Read();

            var contractType = contractRegistry.GetContractType(contractRef.TypeName);
            var contract = GetDelegateForType(contractType, options)(ref reader, contractType, options) ??
                           contractRegistry.CreateContract(contractRef);
            dictionary[contractRef] = contract;
        }

        return new ContractList(dictionary);
    }

    public override void Write(Utf8JsonWriter writer, ContractList value, JsonSerializerOptions options)
    {
        var converter = (JsonConverter<IReadOnlyDictionary<ContractRef, Contract>>)
            options.GetConverter(typeof(IReadOnlyDictionary<ContractRef, Contract>));
        converter.Write(writer, value, options);
    }
}