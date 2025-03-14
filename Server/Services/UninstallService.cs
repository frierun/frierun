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
            var requiredByCount = new Dictionary<Resource, int>();
            foreach (var resource in state.Resources)
            {
                foreach (var dependency in resource.DependsOn)
                {
                    requiredByCount[dependency] = requiredByCount.GetValueOrDefault(dependency) + 1;
                }
            }
            
            var uninstallQueue = new Queue<Resource>();
            uninstallQueue.Enqueue(application);
            while (uninstallQueue.Count > 0)
            {
                var resource = uninstallQueue.Dequeue();
                foreach (var dependency in resource.DependsOn)
                {
                    if (dependency is Application)
                    {
                        continue;
                    }

                    var count = requiredByCount[dependency];
                    if (count == 1)
                    {
                        uninstallQueue.Enqueue(dependency);
                        requiredByCount.Remove(dependency);
                    }
                    else
                    {
                        requiredByCount[dependency] = count - 1;
                    }   
                }
                
                UninstallResource(resource);
            }
            
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