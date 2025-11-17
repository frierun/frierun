using System.Diagnostics;

namespace Frierun.Server.Data;

public class ExecutionPlan(
    Dictionary<ContractRef, Contract> contracts,
    IEnumerable<ExecutionPlan.Alternative> alternatives
) : IExecutionPlan
{
    private readonly HashSet<Application> _requiredApplications = [];

    public record Alternative(ContractRef ContractRef, Contract Contract);

    public ContractList Contracts => new(contracts);

    /// <summary>
    /// List of all contracts that are alternatives to the main execution plan.
    /// </summary>
    public IEnumerable<Alternative> Alternatives => alternatives;

    /// <summary>
    /// Builds the graph of contracts.
    /// </summary>
    private DirectedAcyclicGraph<ContractRef> BuildGraph()
    {
        var graph = new DirectedAcyclicGraph<ContractRef>();
        foreach (var contractRef in contracts.Keys)
        {
            graph.AddVertex(contractRef);
        }

        foreach (var (contractRef, contract) in contracts)
        {
            foreach (var dependency in contract.DependsOn)
            {
                var dependencyRef = dependency.Ref;
                if (dependencyRef == null)
                {
                    continue;
                }

                graph.AddEdge(dependencyRef, contractRef);
            }
        }

        return graph;
    }

    /// <summary>
    /// Get contract by ref.
    /// </summary>
    public Contract GetContract(ContractRef contractRef)
    {
        return contracts[contractRef];
    }

    /// <summary>
    /// Get contract by ref.
    /// </summary>
    public T GetContract<T>(ContractRef<T> contractRef)
        where T : Contract
    {
        return (T)GetContract((ContractRef)contractRef);
    }

    /// <summary>
    /// Get contract by ref.
    /// </summary>
    public Contract GetContract(ContractId contractId)
    {
        var guid = contractId.Guid;
        if (guid != Guid.Empty)
        {
            return contracts.Values.First(contract => contract.Id == guid);
        }

        Debug.Assert(contractId.Ref != null, "Contract ID must have either a GUID or a ContractRef");
        ;
        return contracts[contractId.Ref];
    }

    /// <summary>
    /// Get contract by id.
    /// </summary>
    public T GetContract<T>(ContractId<T> contractId)
        where T : Contract
    {
        return (T)GetContract((ContractId)contractId);
    }


    /// <summary>
    /// Installs all contracts in the execution plan.
    /// </summary>
    /// <param name="state"></param>
    public Application Install(State state)
    {
        var graph = BuildGraph();
        graph.RunDfs(contractRef =>
            {
                var contract = GetContract(contractRef);

                foreach (var argument in contract.GetArguments())
                {
                    argument.Resolve(this);
                }

                if (!contract.Installed)
                {
                    contract = contract.Install(this);
                }

                contracts[contractRef] = contract;

                var handlerApplication = contract.Handler?.Application;
                if (handlerApplication != null)
                {
                    _requiredApplications.Add(handlerApplication);
                }
            }
        );

        var application = CreateApplication();
        contracts.Values.Where(contract => contract is not Application).ToList().ForEach(state.AddContract);
        state.AddContract(application);

        return application;
    }

    /// <summary>
    /// Creates an application from the installed contracts.
    /// </summary>
    private Application CreateApplication()
    {
        var application = contracts.Values.OfType<Application>().First();

        return application with
        {
            RequiredApplications = _requiredApplications.Select(app => app.Name).ToList(),
            ContractRefs = contracts
                .Where(pair => pair.Value is not Application)
                .ToDictionary(
                    pair => pair.Key,
                    pair => pair.Value.Id
                ),
            DependsOn = contracts
                .Where(pair => pair.Value is not Application)
                .Select(pair => new ContractId(pair.Value.Id))
        };
    }
}