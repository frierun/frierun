using Frierun.Server.Models;

namespace Frierun.Server.Services;

public class ParameterService(State state)
{
    /// <summary>
    /// Gets default name for the new package installation
    /// </summary>
    public string GetDefaultName(Package package)
    {
        var name = package.Name;
        var count = 0;
        while (!CanUseName(name, package))
        {
            count++;
            name = $"{package.Name}-{count}"; 
        }
        return name;
    }

    private bool CanUseName(string name, Package package)
    {
        if (state.Applications.Any(application => application.Name == name))
        {
            return false;
        }

        foreach (var volume in package.Volumes ?? Enumerable.Empty<Volume>())
        {
            var volumeName = $"{name}-{volume.Name}";
            if (state.Applications.Any(application => application.VolumeNames?.Contains(volumeName) == true))
            {
                return false;
            }
        }
        
        return true;
    }

    /// <summary>
    /// Gets default port for the new package installation
    /// </summary>
    public int GetDefaultPort()
    {
        var port = 80;
        var count = 0;
        while (state.Applications.Any(application => application.Port == port))
        {
            port = 8080 + count;
            count++;
        }
        return port;
    }
}