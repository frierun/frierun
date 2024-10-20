using Frierun.Server.Models;
using Frierun.Server.Resources;

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
        
        if (state.Resources.OfType<Container>().Any(container => container.Name == name))
        {
            return false;
        }

        foreach (var volume in package.Children.OfType<VolumeDefinition>())
        {
            var volumeName = $"{name}-{volume.Name}";
            if (state.Resources.OfType<Volume>().Any(installedVolume => installedVolume.Name == volumeName))
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
        while (state.Resources.OfType<HttpEndpoint>().Any(httpEndpoint => httpEndpoint.Port == port))
        {
            port = 8080 + count;
            count++;
        }
        return port;
    }
}