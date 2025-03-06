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
            var changedNodes = new Dictionary<Resource, int>();
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
                    
                    if (!changedNodes.TryGetValue(dependency, out var edgesCount))
                    {
                        edgesCount = dependency.RequiredBy.Count;
                    }
                    
                    changedNodes[dependency] = edgesCount - 1;
                    if (edgesCount == 1)
                    {
                        uninstallQueue.Enqueue(dependency);
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
        resource.Uninstall();
        state.RemoveResource(resource);
    }
}