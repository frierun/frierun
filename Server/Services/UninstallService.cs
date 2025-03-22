using Frierun.Server.Data;
using Frierun.Server.Services;

namespace Frierun.Server;

public class UninstallService(
    State state,
    StateSerializer stateSerializer,
    StateManager stateManager,
    InstallerRegistry installerRegistry)
{
    public void Handle(Application application)
    {
        if (!stateManager.StartTask("uninstall"))
        {
            return;
        }

        try
        {
            foreach (var other in state.Applications)
            {
                if (other.RequiredApplications.Contains(application.Name))
                {
                    throw new Exception($"Cannot uninstall {application.Name} because it is required by {other.Name}");
                }
            }
            
            foreach (var resource in application.Resources.Reverse())
            {
                if (resource is Application)
                {
                    throw new Exception("Application cannot contain another application");
                }
                
                UninstallResource(resource);
            }

            UninstallResource(application);
            state.RemoveApplication(application);
            
            stateSerializer.Save(state);
        }
        finally
        {
            stateManager.FinishTask();
        }
    }

    /// <summary>
    /// Uninstalls a resource
    /// </summary>
    private void UninstallResource(Resource resource)
    {
        var uninstaller = installerRegistry.GetUninstaller(resource.GetType());
        if (uninstaller == null)
        {
            throw new Exception($"Uninstaller not found for resource type {resource.GetType()}");
        }
        uninstaller.Uninstall(resource);
    }
}