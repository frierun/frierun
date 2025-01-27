using System.Runtime.InteropServices;

namespace Frierun.Server.Services;

public static class Storage
{
    public static string DirectoryName { get; }
    
    static Storage()
    {
        var localData = Environment.GetEnvironmentVariable(
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? "LocalAppData"
                : "Home"
        );
        
        DirectoryName = Path.Combine(localData ?? "", "Frierun");
        if (!Directory.Exists(DirectoryName))
        {
            Directory.CreateDirectory(DirectoryName);
        }
    }
}