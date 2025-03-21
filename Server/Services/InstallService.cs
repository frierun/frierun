using Frierun.Server.Data;
using Frierun.Server.Services;

namespace Frierun.Server;

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
            
            state.AddResource(application);
            foreach (var resource in application.DependsOn)
            {
                state.AddResource(resource);
            }
            
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