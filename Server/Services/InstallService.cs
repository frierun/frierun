using Frierun.Server.Data;

namespace Frierun.Server.Services;

public class InstallService(
    State state,
    StateSerializer stateSerializer,
    StateManager stateManager,
    ILogger<InstallService> logger)
{
    public Application? Handle(IExecutionPlan executionPlan)
    {
        if (!stateManager.StartTask("install"))
        {
            return null;
        }

        try
        {
            var application = executionPlan.Install();
            stateSerializer.Save(state);
            return application;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to install");
            return null;
        }
        finally
        {
            stateManager.FinishTask();
        }
    }
}