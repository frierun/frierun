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
        var contracts = new Dictionary<ContractId, Contract>(application.Contracts);
        while (contracts.Count > 0)
        {
            var contractId = contracts
                .First(pair => !contracts.Any(dependPair => dependPair.Value.DependsOn.Contains(pair.Key))).Key;

            contracts[contractId].Uninstall();
            contracts.Remove(contractId);
        }
    }
}