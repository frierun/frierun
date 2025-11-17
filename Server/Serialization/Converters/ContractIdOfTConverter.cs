using System.Diagnostics;
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
            if (value is null)
            {
                return null;
            }

            if (Guid.TryParse(value, out var guid))
            {
                return new ContractId<TContract>(guid);
            }

            return new ContractId<TContract>(Guid.Empty, value);
        }

        public override void Write(Utf8JsonWriter writer, ContractId<TContract> value, JsonSerializerOptions options)
        {
            if (value.Guid != Guid.Empty)
            {
                writer.WriteStringValue(value.Guid.ToString());
                return;
            }

            var contractRef = value.Ref ?? new ContractRef<TContract>();
            var contractRefConverter = (JsonConverter<ContractRef<TContract>>)options.GetConverter(typeof(ContractRef<TContract>));
            contractRefConverter.Write(writer, contractRef, options);
        }
    }
}