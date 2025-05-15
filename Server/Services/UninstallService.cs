using System.Diagnostics;
using Frierun.Server.Data;

namespace Frierun.Server;

public class UninstallService(
    State state,
    StateSerializer stateSerializer,
    StateManager stateManager)
{
    public void Handle(Application application)
    {
        if (!stateManager.StartTask("uninstall"))
        {
            return;
        }

        try
        {
            foreach (var other in state.Applications)
            {
                if (other.RequiredApplications.Contains(application.Name))
                {
                    throw new Exception($"Cannot uninstall {application.Name} because it is required by {other.Name}");
                }
            }
            
            foreach (var contract in application.Contracts.Reverse())
            {
                Debug.Assert(contract is not Package);
                
                if (contract is IHasResult hasResult)
                {
                    Debug.Assert(hasResult.Result != null);
                    
                    hasResult.Result.Uninstall();
                }
            }

            application.Uninstall();
            state.RemoveApplication(application);
            
            stateSerializer.Save(state);
        }
        finally
        {
            stateManager.FinishTask();
        }
    }
}