using Frierun.Server.Models;
using Frierun.Server.Resources;

namespace Frierun.Server.Services;

public class InstallService(
    State state,
    StateSerializer stateSerializer,
    StateManager stateManager,
    ILogger<InstallService> logger
)
{
    public void Handle(ExecutionPlan executionPlan)
    {
        if (!stateManager.StartTask("install"))
        {
            return;
        }

        try
        {
            var resources = new List<Resource>();
            foreach (var resourceDefinition in executionPlan.ResourcesToInstall)
            {
                var provider = executionPlan.Providers[resourceDefinition];

                var resource = provider.Install(
                    executionPlan,
                    provider.GetParameters(executionPlan, resourceDefinition),
                    resourceDefinition
                ) as Resource;
                if (resource == null)
                {
                    logger.LogError("Provider {ProviderType} failed to create resource",
                        resourceDefinition.ResourceType);
                    return;
                }

                resources.Add(resource);
            }

            state.Applications.Add(new Application(Guid.NewGuid(), executionPlan.Name, resources, executionPlan.Package));
            stateSerializer.Save(state);
        }
        catch (Exception e)
        {
            logger.LogError("Failed to install package {PackageName}: {ErrorMessage}", executionPlan.Package.Name, e.Message);
        }
        finally
        {
            stateManager.FinishTask();
        }
    }
}