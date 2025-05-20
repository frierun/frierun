using Frierun.Server.Data;
using Frierun.Server.Handlers;

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
        catch (HandlerException e)
        {
            stateManager.Error = e.Result;
            return null;
        }
        finally
        {
            stateManager.FinishTask();
        }
    }
}