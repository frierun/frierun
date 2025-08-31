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

            UninstallContracts(application);

            state.RemoveApplication(application);

            stateSerializer.Save(state);
        }
        finally
        {
            stateManager.FinishTask();
        }
    }

    private void UninstallContracts(Application application)
    {
        var contracts = application.Contracts.ToList();
        while (contracts.Count > 0)
        {
            var contract = contracts.First(contract => !contracts.Any(depend => depend.DependsOn.Contains(contract.Id)));
            contract.Uninstall();
            contracts.Remove(contract);
        }
    }
}