using Frierun.Server.Data;
using Frierun.Server.Handlers;

namespace Frierun.Server;

public class ExecutionService(
    HandlerRegistry handlerRegistry,
    ContractRegistry contractRegistry,
    State state
)
{
    private record StackItem(DiscoveryGraph Graph, ContractId ContractId, Queue<ContractList> Queue);

    /// <summary>
    /// Creates an execution plan for the given package.
    /// </summary>
    /// <exception cref="HandlerException"></exception>
    public ExecutionPlan Create(Package package)
    {
        var branchesStack = new Stack<StackItem>();
        DiscoveryGraph? currentGraph = new DiscoveryGraph();
        var applicationName = GetApplicationName(package);

        ContractId? nextId = package.Id;
        Contract? nextContract = package;

        while (nextId != null)
        {
            nextContract ??= contractRegistry.CreateContract(nextId);

            var branches = new Queue<ContractList>(DiscoverContract(nextContract, applicationName));
            if (branches.Count != 0)
            {
                branchesStack.Push(new StackItem(currentGraph, nextId, branches));
            }

            while (true)
            {
                var (item, branch) = PopNextBranch(branchesStack);
                if (branch == null || item == null)
                {
                    throw new HandlerNotFoundException(nextContract);
                }

                currentGraph = item.Graph;

                if (currentGraph.Apply(item.ContractId, branch))
                {
                    break;
                }
            }

            (nextId, nextContract) = currentGraph.Next();
        }

        var alternatives = branchesStack
            .SelectMany(item => item.Queue)
            .Select(result => result.Values.Single(contract => contract.Handler != null))
            .ToList();

        return new ExecutionPlan(currentGraph.Contracts, alternatives);
    }

    /// <summary>
    /// Pops the next branch from the stack.
    /// </summary>
    /// <returns>nulls if no more branches found</returns>
    private (StackItem? item, ContractList? branch) PopNextBranch(Stack<StackItem> branchesStack)
    {
        // no variants found for that contract, rollback to the previous branching point
        if (branchesStack.Count == 0)
        {
            return (null, null);
        }

        var item = branchesStack.Pop();

        var branch = item.Queue.Dequeue();

        if (item.Queue.Count > 0)
        {
            branchesStack.Push(item with { Graph = new DiscoveryGraph(item.Graph) });
        }

        return (item, branch);
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
    private IEnumerable<ContractList> DiscoverContract(Contract contract, string? prefix = null)
    {
        var context = new ApplicationContext(
            contract.Name, 
            prefix ?? ""
        );
        
        if (contract.Handler != null)
        {
            return contract.Handler.Initialize(contract, context);
        }

        return handlerRegistry
            .GetHandlers(contract.GetType())
            .Where(handler =>
                contract.HandlerApplication == null || handler.Application?.Name == contract.HandlerApplication
            )
            .SelectMany(handler => handler.Initialize(contract, context));
    }
}