using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Frierun.Server.Data;

namespace Frierun.Server;

public class ContractIdOfTConverter : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        if (!typeToConvert.IsGenericType)
        {
            return false;
        }

        return typeToConvert.GetGenericTypeDefinition() == typeof(ContractId<>);
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var typeArguments = typeToConvert.GetGenericArguments();
        var contractType = typeArguments[0];

        var converter = (JsonConverter)Activator.CreateInstance(
            typeof(ContractIdOfTConverterInner<>).MakeGenericType([contractType])
        )!;

        return converter;
    }

    private class ContractIdOfTConverterInner<TContract> : JsonConverter<ContractId<TContract>>
        where TContract : Contract
    {
        public override ContractId<TContract>? Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options
        )
        {
            var value = reader.GetString();
            var name = "";
            if (value != null)
            {
                var parts = value.Split(':', 2);
                name = parts[0];
            }
        
            return (ContractId<TContract>)ContractId.Create(typeof(TContract), name);
        }

        public override void Write(Utf8JsonWriter writer, ContractId<TContract> value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.Name);
        }
    }
}