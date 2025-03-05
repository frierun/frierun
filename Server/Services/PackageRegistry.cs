using Frierun.Server.Data;
using Frierun.Server.Services;

namespace Frierun.Server;

public class PackageRegistry(PackageSerializer packageSerializer)
{
    public IList<Package> Packages { get; } = [];

    /// <summary>
    /// Loads packages from the "Packages" directory in the same directory as the executing assembly.
    /// </summary>
    public void Load()
    {
        foreach (var package in packageSerializer.Load().OrderBy(package => package.Name))
        {
            Packages.Add(package);
        }
    }

    public Package? Find(string name)
    {
        return Packages.FirstOrDefault(p => p.Name == name);
    }
}