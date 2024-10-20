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
            var resource = executionPlan.Install();
            state.Applications.Add((Application)resource);
            stateSerializer.Save(state);
        }
        catch (Exception e)
        {
            logger.LogError("Failed to install package: {ErrorMessage}", e.Message);
        }
        finally
        {
            stateManager.FinishTask();
        }
    }
}