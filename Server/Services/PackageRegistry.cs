using Frierun.Server.Models;

namespace Frierun.Server.Services;

public class PackageRegistry
{
    public IList<Package> Packages { get; } = [
        new("Heimdall", "lscr.io/linuxserver/heimdall:latest", 80),
    ];
    
    public Package? Find(string name)
    {
        return Packages.FirstOrDefault(p => p.Name == name);
    }
}