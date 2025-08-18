using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Frierun.Server.Data;

namespace Frierun.Server;

public class ArgumentOfTConverter : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        if (!typeToConvert.IsGenericType)
        {
            return false;
        }

        return typeToConvert.GetGenericTypeDefinition() == typeof(Argument<>);
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var typeArguments = typeToConvert.GetGenericArguments();
        var contractType = typeArguments[0];

        var converter = (JsonConverter)Activator.CreateInstance(
            typeof(ArgumentOfTConverterInner<>).MakeGenericType(contractType),
            BindingFlags.Instance | BindingFlags.Public,
            binder: null,
            args: [options],
            culture: null
        )!;

        return converter;
    }

    private class ArgumentOfTConverterInner<T>(JsonSerializerOptions options) : JsonConverter<Argument<T>>
    {
        private readonly JsonConverter<T?> _valueConverter = (JsonConverter<T?>)options.GetConverter(typeof(T?));

        public override void Write(Utf8JsonWriter writer, Argument<T> value, JsonSerializerOptions options)
        {
            _valueConverter.Write(writer, value.Value, options);
        }

        public override Argument<T> Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options
        )
        {
            var value = _valueConverter.Read(ref reader, typeof(T), options);
            return new Argument<T>(value);
        }
    }
}