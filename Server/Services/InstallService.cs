using Frierun.Server.Data;

namespace Frierun.Server.Services;

public class InstallService(
    State state,
    StateSerializer stateSerializer,
    StateManager stateManager,
    ProviderRegistry providerRegistry,
    ILogger<InstallService> logger)
{
    public void Handle(ExecutionPlan executionPlan)
    {
        if (!stateManager.StartTask("install"))
        {
            return;
        }

        try
        {
            executionPlan.Install();
            var application = executionPlan.GetResource<Application>(executionPlan.RootContractId);
            stateSerializer.Save(state);
            if (application.Package?.Name == "traefik")
            {
                providerRegistry.UseTraefik(application);
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to install");
        }
        finally
        {
            stateManager.FinishTask();
        }
    }
}