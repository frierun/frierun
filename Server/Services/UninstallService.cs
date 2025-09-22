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
        var contractRefs = new HashSet<Guid>(application.ContractRefs.Values);
        while (contractRefs.Count > 0)
        {
            var guid = contractRefs
                .First(guid => state.ContractsById.Values.All(depend => !depend.DependsOn.Contains(guid)));

            var contract = state.GetContract(guid);
            contract.Uninstall();
            state.RemoveContract(contract);
            contractRefs.Remove(guid);
        }
    }
}