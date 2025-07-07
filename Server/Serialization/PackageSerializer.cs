using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Frierun.Server.Data;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.TypeResolvers;
using File = System.IO.File;

namespace Frierun.Server;

public class PackageSerializer(ILogger<PackageSerializer> logger, ContractRegistry contractRegistry)
{
    private readonly JsonSerializerOptions _serializerOptions = new()
    {
        Converters =
        {
            new ContainerMountConverter(),
            new ContractIdConverter(contractRegistry),
            new ContractIdOfTConverter(),
            new YamlBoolConverter()
        },
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        TypeInfoResolver = new DefaultJsonTypeInfoResolver
        {
            Modifiers =
            {
                JsonExtensions.UnIgnorePropertiesForDeserialize(
                    typeof(Contract),
                    nameof(Contract.DependsOn),
                    nameof(Contract.DependencyOf)
                )
            }
        }
    };

    /// <summary>
    /// Loads packages from the "Packages" directory.
    /// </summary>
    public IEnumerable<Package> LoadAll()
    {
        var packagesDirectory = Path.Combine(AppContext.BaseDirectory, "Packages");

        if (!Directory.Exists(packagesDirectory))
        {
            yield break;
        }

        foreach (var fileName in Directory.EnumerateFiles(packagesDirectory))
        {
            if (!fileName.EndsWith(".json") && !fileName.EndsWith(".yaml") && !fileName.EndsWith(".yml"))
            {
                continue;
            }
            
            using Stream stream = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            var package = Load(stream);
            if (package is null)
            {
                logger.LogWarning("Failed to deserialize package from {FileName}", fileName);
                continue;
            }

            yield return package;
        }
    }

    /// <summary>
    /// Loads a package from a stream.
    /// </summary>
    public Package? Load(Stream stream)
    {
        var deserializer = new DeserializerBuilder()
            .WithTypeResolver(new StaticTypeResolver())
            .Build();
        
        using StreamReader reader = new(stream);
        
        var obj = deserializer.Deserialize(reader);
        
        var json = JsonSerializer.SerializeToDocument(obj);
        return json.Deserialize<Package>(_serializerOptions);
    }
}