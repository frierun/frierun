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
            foreach (var resource in application.DependsOn.Reverse())
            {
                UninstallResource(resource);
            }

            UninstallResource(application);
            
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
        state.RemoveResource(resource);
    }
}