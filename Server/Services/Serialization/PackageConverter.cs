﻿using System.Text.Json;
using System.Text.Json.Serialization;
using Frierun.Server.Data;

namespace Frierun.Server.Services;

public class PackageConverter(PackageRegistry packageRegistry) : JsonConverter<Package>
{
    /// <inheritdoc />
    public override Package? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var name = reader.GetString();
        if (name is null)
        {
            return null;
        }
        return packageRegistry.Find(name);
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, Package value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Name);
    }
}