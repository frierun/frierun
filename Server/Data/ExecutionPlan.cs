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
            foreach (var dependency in contract.GetDependencies())
            {
                var dependencyRef = dependency.Ref ?? dependency.DefaultRef;
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
    /// Get contract by id.
    /// </summary>
    public T GetContract<T>(ContractId<T> contractId)
        where T : Contract
    {
        if (contractId.Guid != Guid.Empty)
        {
            return contracts.Values.OfType<T>().First(contract => contract.Id == contractId.Guid);
        }

        var contractRef = contractId.TypedRef;
        return (T)contracts[contractRef];
    }


    /// <summary>
    /// Installs all contracts in the execution plan.
    /// </summary>
    public Application Install(State state)
    {
        var graph = BuildGraph();
        graph.RunDfs(contractRef =>
            {
                var contract = contracts[contractRef];
                contract = contract.ResolveArguments(this);
                contracts[contractRef] = contract;

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