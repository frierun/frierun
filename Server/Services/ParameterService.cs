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
        while (state.Applications.Any(application => application.Name == name))
        {
            count++;
            name = $"{package.Name}-{count}"; 
        }
        return name;
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