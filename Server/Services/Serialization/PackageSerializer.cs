using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Frierun.Server.Data;
using File = System.IO.File;

namespace Frierun.Server;

public class PackageSerializer(ILogger<PackageSerializer> logger, ContractRegistry contractRegistry)
{
    private readonly JsonSerializerOptions _serializerOptions = new()
    {
        Converters = { new ContractIdConverter(contractRegistry) },
        TypeInfoResolver = new DefaultJsonTypeInfoResolver()
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
        var assemblyLocation = Assembly.GetExecutingAssembly().Location;
        var assemblyDirectory = Path.GetDirectoryName(assemblyLocation) ?? throw new InvalidOperationException();
        var packagesDirectory = Path.Combine(assemblyDirectory, "Packages");

        if (!Directory.Exists(packagesDirectory))
        {
            yield break;
        }

        foreach (var fileName in Directory.EnumerateFiles(packagesDirectory, "*.json"))
        {
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
        return JsonSerializer.Deserialize<Package>(stream, _serializerOptions);
    }
}