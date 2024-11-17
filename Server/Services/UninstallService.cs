using Frierun.Server.Data;

namespace Frierun.Server.Services;

public class UninstallService(
    State state,
    StateSerializer stateSerializer,
    StateManager stateManager,
    ProviderRegistry providerRegistry,
    ILogger<UninstallService> logger)
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
        var providers = providerRegistry.Get(resource.GetType());
        if (providers.Count > 1)
        {
            logger.LogError("Multiple providers found for resource type {ResourceType}", resource.GetType().Name);
            return;
        }
        
        if (providers.Count == 0)
        {
            logger.LogError("No providers found for resource type {ResourceType}", resource.GetType().Name);
            return;
        }

        var provider = providers[0];
        provider.Uninstall(resource);
        resource.Uninstall();
        state.Resources.Remove(resource);
    }
}