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
            var application = (Application)executionPlan.Install();
            stateSerializer.Save(state);
            if (application.Package?.Name == "traefik")
            {
                providerRegistry.UseTraefik(application);
            }
        }
        finally
        {
            stateManager.FinishTask();
        }
    }
}