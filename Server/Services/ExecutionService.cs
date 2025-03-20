using Frierun.Server.Data;
using Frierun.Server.Installers;
using Frierun.Server.Services;

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
            package,
            contracts,
            state,
            installerRegistry
        );
    }

    /// <summary>
    /// Discovers and collects all contracts required to fulfill the package.
    /// </summary>
    private Dictionary<ContractId, Contract> DiscoverContracts(Package package)
    {
        var branchesStack = new Stack<(DiscoveryGraph graph, Queue<InstallerInitializeResult> queue)>();
        var discoveryGraph = new DiscoveryGraph();

        discoveryGraph.Apply(DiscoverContract(package).First());
        var initializedPackage = (Package)discoveryGraph.Contracts[package];
        var prefix = initializedPackage.Prefix ?? initializedPackage.Name;

        var (nextId, nextContract) = discoveryGraph.Next();
        while (nextId != null)
        {
            nextContract ??= contractRegistry.CreateContract(nextId);

            var branches = new Queue<InstallerInitializeResult>(DiscoverContract(nextContract, prefix));
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