using Frierun.Server.Data;
using Frierun.Server.Installers;

namespace Frierun.Server;

public class ExecutionService(
    InstallerRegistry installerRegistry,
    ContractRegistry contractRegistry,
    State state
)
{
    /// <summary>
    /// Creates an execution plan for the given package.
    /// </summary>
    /// <exception cref="InstallerNotFoundException"></exception>
    public ExecutionPlan Create(Package package)
    {
        var contracts = DiscoverContracts(package);
        return new ExecutionPlan(
            contracts,
            installerRegistry
        );
    }
    
    /// <summary>
    /// Gets the application name from the package.
    /// </summary>
    private string GetApplicationName(Package package)
    {
        if (package.Prefix != null)
        {
            if (state.Applications.Any(application => application.Name == package.Prefix))
            {
                throw new Exception("Application with the same name already exists");
            }

            return package.Prefix;
        }
        
        var count = 1;
        var applicationName = package.Name;
        while (state.Applications.Any(application => application.Name == applicationName))
        {
            count++;
            applicationName = $"{package.Name}{count}";
        }

        return applicationName;
    }

    /// <summary>
    /// Discovers and collects all contracts required to fulfill the package.
    /// </summary>
    private Dictionary<ContractId, Contract> DiscoverContracts(Package package)
    {
        var branchesStack = new Stack<(DiscoveryGraph graph, Queue<InstallerInitializeResult> queue)>();
        var discoveryGraph = new DiscoveryGraph();
        var applicationName = GetApplicationName(package);

        ContractId? nextId = package.Id;
        Contract? nextContract = package;
        
        while (nextId != null)
        {
            nextContract ??= contractRegistry.CreateContract(nextId);

            var branches = new Queue<InstallerInitializeResult>(DiscoverContract(nextContract, applicationName));
            if (branches.Count == 0)
            {
                // no variants found for that contract, rollback to the previous branching point
                if (branchesStack.Count == 0)
                {
                    throw new InstallerNotFoundException(nextContract);
                }

                (discoveryGraph, branches) = branchesStack.Pop();
            }

            var branch = branches.Dequeue();

            if (branches.Count > 0)
            {
                branchesStack.Push((new DiscoveryGraph(discoveryGraph), branches));
            }

            discoveryGraph.Apply(branch);
            (nextId, nextContract) = discoveryGraph.Next();
        }

        return discoveryGraph.Contracts;
    }

    /// <summary>
    /// Discovers all possible dependent contracts for the given contract.
    /// </summary>
    private IEnumerable<InstallerInitializeResult> DiscoverContract(Contract contract, string? prefix = null)
    {
        return installerRegistry
            .GetInstallers(contract.GetType(), contract.Installer)
            .SelectMany(installer => installer.Initialize(contract, prefix ?? ""));
    }
}