using Frierun.Server.Data;
using Frierun.Server.Installers;

namespace Frierun.Server.Services;

public class ExecutionService(
    InstallerRegistry installerRegistry,
    ContractRegistry contractRegistry,
    State state
)
{
    /// <summary>
    /// Creates an execution plan for the given package.
    /// </summary>
    public ExecutionPlan Create(Package package)
    {
        var contracts = CollectContracts(package);
        return new ExecutionPlan(
            package,
            contracts,
            state,
            installerRegistry
        );
    }

    /// <summary>
    /// Collects all contracts in the package.
    /// </summary>
    private Dictionary<ContractId, Contract> CollectContracts(Package package)
    {
        var emptyContracts = new HashSet<ContractId>();
        var queue = new Queue<Contract>();
        var contracts = new Dictionary<ContractId, Contract>();
        string prefix = package.Prefix ?? package.Name;
        queue.Enqueue(package);


        while (queue.Count > 0 || emptyContracts.Count > 0)
        {
            if (queue.Count == 0)
            {
                var contractId = emptyContracts.First();
                emptyContracts.Remove(contractId);
                if (contracts.ContainsKey(contractId))
                {
                    continue;
                }

                queue.Enqueue(contractRegistry.CreateContract(contractId));
            }

            var contract = queue.Dequeue();
            if (contracts.ContainsKey(contract.Id))
            {
                continue;
            }

            var result = GetInstaller(contract).Initialize(contract, prefix, state);

            contracts[contract.Id] = result.Contract;
            if (contract is Package initializedPackage)
            {
                prefix = initializedPackage.Prefix ?? initializedPackage.Name;
            }

            foreach (var additionalContract in result.AdditionalContracts)
            {
                queue.Enqueue(additionalContract);
            }

            foreach (var contractId in result.RequiredContracts)
            {
                if (!contracts.ContainsKey(contractId))
                {
                    emptyContracts.Add(contractId);
                }
            }
        }

        return contracts;
    }

    /// <summary>
    /// Gets installer for the contract.
    /// </summary>
    private IInstaller GetInstaller(Contract contract)
    {
        var installer = installerRegistry.GetInstaller(contract.GetType(), contract.Installer);
        if (installer == null)
        {
            throw contract.Installer == null
                ? new Exception($"Can't find default installer for resource {contract.GetType()}")
                : new Exception($"Can't find installer '{contract.Installer}' for resource {contract.GetType()}");
        }

        return installer;
    }
}