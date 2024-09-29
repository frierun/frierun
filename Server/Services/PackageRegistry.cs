using Frierun.Server.Models;
using Newtonsoft.Json;

namespace Frierun.Server.Services;

public class PackageRegistry(ILogger<PackageRegistry> logger)
{
    public IList<Package> Packages { get; } = [];

    /// <summary>
    /// Loads packages from the "Packages" directory in the same directory as the executing assembly.
    /// </summary>
    public void Load()
    {
        var assemblyLocation = System.Reflection.Assembly.GetEntryAssembly()?.Location ??
                               throw new InvalidOperationException();
        var assemblyDirectory = Path.GetDirectoryName(assemblyLocation) ??
                                throw new InvalidOperationException();
        var packagesDirectory = Path.Combine(assemblyDirectory, "Packages");

        if (Directory.Exists(packagesDirectory))
        {
            foreach (var fileName in Directory.EnumerateFiles(packagesDirectory, "*.json"))
            {
                var package = JsonConvert.DeserializeObject<Package>(File.ReadAllText(fileName));
                if (package == null)
                {
                    logger.LogWarning("Failed to load package from {FileName}", fileName);
                }
                else
                {
                    Packages.Add(package);
                }
            }
        }
    }

    public Package? Find(string name)
    {
        return Packages.FirstOrDefault(p => p.Name == name);
    }
}