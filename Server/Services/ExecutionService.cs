using Frierun.Server.Data;
using Frierun.Server.Handlers;

namespace Frierun.Server;

public class ExecutionService(
    HandlerRegistry handlerRegistry,
    ContractRegistry contractRegistry,
    State state
)
{
    private Stack<(DiscoveryGraph graph, Queue<ContractInitializeResult> queue)> _branchesStack = new();
    private DiscoveryGraph _currentGraph = new();
    
    /// <summary>
    /// Creates an execution plan for the given package.
    /// </summary>
    /// <exception cref="HandlerException"></exception>
    public ExecutionPlan Create(Package package)
    {
        _branchesStack = new Stack<(DiscoveryGraph graph, Queue<ContractInitializeResult> queue)>();
        _currentGraph = new DiscoveryGraph();
        var applicationName = GetApplicationName(package);

        ContractId? nextId = package.Id;
        Contract? nextContract = package;

        while (nextId != null)
        {
            nextContract ??= contractRegistry.CreateContract(nextId);

            var branches = new Queue<ContractInitializeResult>(DiscoverContract(nextContract, applicationName));
            if (branches.Count != 0)
            {
                _branchesStack.Push((_currentGraph, branches));
            }

            while (true)
            {
                var branch = PopNextBranch();
                if (branch == null)
                {
                    throw new HandlerNotFoundException(nextContract);
                }

                if (_currentGraph.Apply(branch))
                {
                    break;
                }
            }
            (nextId, nextContract) = _currentGraph.Next();
        }

        var alternatives = _branchesStack
            .SelectMany(pair => pair.queue)
            .Select(result => result.Contract)
            .ToList();
        
        return new ExecutionPlan(_currentGraph.Contracts, alternatives);
    }

    /// <summary>
    /// Pops next branch from the stack. Also sets _currentGraph to the correct value
    /// </summary>
    /// <returns>null if no more branches found</returns>
    private ContractInitializeResult? PopNextBranch()
    {
        // no variants found for that contract, rollback to the previous branching point
        if (_branchesStack.Count == 0)
        {
            return null;
        }

        var (graph, branches) = _branchesStack.Pop();
        _currentGraph = graph;

        var branch = branches.Dequeue();

        if (branches.Count > 0)
        {
            _branchesStack.Push((new DiscoveryGraph(_currentGraph), branches));
        }

        return branch;
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
    /// Discovers all possible dependent contracts for the given contract.
    /// </summary>
    private IEnumerable<ContractInitializeResult> DiscoverContract(Contract contract, string? prefix = null)
    {
        if (contract.Handler != null)
        {
            return contract.Handler.Initialize(contract, prefix ?? "");
        }
        
        return handlerRegistry
            .GetHandlers(contract.GetType())
            .Where(handler => contract.HandlerApplication == null || handler.Application?.Name == contract.HandlerApplication)
            .SelectMany(handler => handler.Initialize(contract, prefix ?? ""));
    }
}