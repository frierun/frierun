using System.Text.Json;
using Frierun.Server.Models;
using Frierun.Server.Resources;

namespace Frierun.Server.Services;

public class PackageSerializer(ILogger<PackageSerializer> logger)
{
    private readonly JsonSerializerOptions _serializerOptions = new();

    /// <summary>
    /// Loads packages from the "Packages" directory.
    /// </summary>
    public IEnumerable<Package> Load()
    {
        var assemblyLocation = System.Reflection.Assembly.GetEntryAssembly()?.Location ??
                               throw new InvalidOperationException();
        var assemblyDirectory = System.IO.Path.GetDirectoryName(assemblyLocation) ??
                                throw new InvalidOperationException();
        var packagesDirectory = System.IO.Path.Combine(assemblyDirectory, "Packages");
     
        if (!Directory.Exists(packagesDirectory))
        {
            yield break;
        }
        
        foreach (var fileName in Directory.EnumerateFiles(packagesDirectory, "*.json"))
        {
            using Stream stream = File.Open(fileName, FileMode.Open, FileAccess.Read);
            var package = JsonSerializer.Deserialize<Package>(stream, _serializerOptions);
            if (package is null)
            {
                logger.LogWarning("Failed to deserialize package from {FileName}", fileName);
                continue;
            }
            
            yield return package;
        }
    }
}