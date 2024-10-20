using Frierun.Server.Models;
using Frierun.Server.Providers;
using Frierun.Server.Resources;

namespace Frierun.Server.Services;

public class InstallService(
    State state,
    StateSerializer stateSerializer,
    StateManager stateManager,
    ProviderRegistry providerRegistry
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
            var resource = (Application)executionPlan.Install();
            state.Applications.Add(resource);
            stateSerializer.Save(state);
            if (resource.Package?.Name == "traefik")
            {
                providerRegistry.UseTraefik();
            }
        }
        finally
        {
            stateManager.FinishTask();
        }
    }
}