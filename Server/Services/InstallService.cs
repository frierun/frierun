using Frierun.Server.Data;
using Frierun.Server.Services;

namespace Frierun.Server;

public class InstallService(
    State state,
    StateSerializer stateSerializer,
    StateManager stateManager)
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
            state.AddApplication(application);
            stateSerializer.Save(state);
            return application;
        }
        finally
        {
            stateManager.FinishTask();
        }
    }
}