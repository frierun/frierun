using System.Text.Json;
using System.Text.Json.Serialization;

namespace Frierun.Server;

public class YamlBoolConverter : JsonConverter<bool>
{
    private readonly HashSet<string> _trueValues = new(StringComparer.OrdinalIgnoreCase)
    {
        "true",
        "yes",
        "on",
        "1"
    };
    
    private readonly HashSet<string> _falseValues = new(StringComparer.OrdinalIgnoreCase)
    {
        "false",
        "no",
        "off",
        "0"
    };
    
    public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        if (value is null)
        {
            throw new Exception("Invalid boolean value");
        }

        if (_trueValues.Contains(value))
        {
            return true;
        }
        
        if (_falseValues.Contains(value))
        {
            return false;
        }
        
        throw new Exception($"Invalid boolean value: {value}");
    }

    public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
    {
        throw new NotSupportedException();
    }
}