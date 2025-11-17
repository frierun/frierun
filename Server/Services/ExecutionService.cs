using Frierun.Server.Data;
using Frierun.Server.Handlers;

namespace Frierun.Server;

public class ExecutionService(
    HandlerRegistry handlerRegistry,
    ContractRegistry contractRegistry,
    State state
)
{
    private record StackItem(DiscoveryGraph Graph, ContractRef ContractRef, Queue<ContractList> Queue);

    /// <summary>
    /// Creates an execution plan for the given application.
    /// </summary>
    /// <exception cref="HandlerException"></exception>
    public ExecutionPlan Create(Application application)
    {
        var branchesStack = new Stack<StackItem>();
        var currentGraph = new DiscoveryGraph();
        var applicationName = GetApplicationName(application);

        ContractRef? nextId = new ContractRef<Application>(applicationName);
        Contract? nextContract = application with { Name = applicationName };

        while (nextId != null)
        {
            nextContract ??= contractRegistry.CreateContract(nextId);

            var contractLists = DiscoverContract(nextId, nextContract, applicationName);
            var branches = new Queue<ContractList>(contractLists);
            if (branches.Count != 0)
            {
                branchesStack.Push(new StackItem(currentGraph, nextId, branches));
            }

            while (true)
            {
                var (item, branch) = PopNextBranch(branchesStack);
                if (branch == null || item == null)
                {
                    throw new HandlerNotFoundException(nextId, nextContract);
                }

                currentGraph = item.Graph;

                if (currentGraph.Apply(item.ContractRef, branch))
                {
                    break;
                }
            }

            (nextId, nextContract) = currentGraph.Next();
        }

        var alternatives = branchesStack
            .SelectMany(item => item.Queue.Select(contracts =>
                    new ExecutionPlan.Alternative(item.ContractRef, contracts[item.ContractRef])
                )
            )
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
    private string GetApplicationName(Application application)
    {
        if (application.Name != "")
        {
            if (state.Applications.Any(app => app.Name == application.Name))
            {
                throw new HandlerException(
                    "Application with the same name already exists",
                    "Choose a different name for the application or remove it",
                    application
                );
            }

            return application.Name;
        }

        var count = 1;
        var packageName = application.Package?.Name ?? throw new Exception("Package not found");
        var applicationName = packageName;
        while (state.Applications.Any(app => app.Name == applicationName))
        {
            count++;
            applicationName = $"{packageName}{count}";
        }

        return applicationName;
    }

    /// <summary>
    /// Discovers all possible dependent contracts for the given contract.
    /// </summary>
    private IEnumerable<ContractList> DiscoverContract(
        ContractRef contractRef,
        Contract contract,
        string? prefix = null
    )
    {
        var context = new ApplicationContext(
            contractRef.Name,
            prefix ?? ""
        );

        if (contract.Id != Guid.Empty)
        {
            var installedContract = state.GetContract(contract.Id);
            if (!installedContract.IsSubset(contract))
            {
                return [];
            }

            return [new ContractList { [context] = installedContract }];
        }

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