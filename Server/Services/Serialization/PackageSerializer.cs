﻿using System.Reflection;
using System.Text.Json;
using Frierun.Server.Data;
using File = System.IO.File;

namespace Frierun.Server.Services;

public class PackageSerializer(ILogger<PackageSerializer> logger, ContractRegistry contractRegistry)
{
    private readonly JsonSerializerOptions _serializerOptions = new()
    {
        Converters = { new ContractIdConverter(contractRegistry) }

    };

    /// <summary>
    /// Loads packages from the "Packages" directory.
    /// </summary>
    public IEnumerable<Package> Load()
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