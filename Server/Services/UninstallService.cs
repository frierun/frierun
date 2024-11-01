using Frierun.Server.Models;
using Frierun.Server.Resources;

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
            foreach (var container in application.AllResources.OfType<Container>())
            {
                UninstallResource(container);
            }

            foreach (var volume in application.AllResources.OfType<Volume>())
            {
                UninstallResource(volume);
            }

            foreach (var volume in application.AllResources.OfType<Volume>())
            {
                UninstallResource(volume);
            }

            foreach (var resource in application.AllResources.OfType<TraefikHttpEndpoint>())
            {
                UninstallResource(resource);
            }
            
            foreach (var containerGroup in application.AllResources.OfType<ContainerGroup>())
            {
                UninstallResource(containerGroup);
            }

            state.Applications.Remove(application);
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
    }
}