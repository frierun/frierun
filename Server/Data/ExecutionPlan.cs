namespace Frierun.Server.Data;

public class ExecutionPlan(
    Dictionary<ContractId, Contract> contracts,
    IEnumerable<ExecutionPlan.Alternative> alternatives
) : IExecutionPlan
{
    private readonly HashSet<Application> _requiredApplications = [];

    public record Alternative(ContractId ContractId, Contract Contract);

    public ContractList Contracts => new(contracts);

    /// <summary>
    /// List of all contracts that are alternatives to the main execution plan.
    /// </summary>
    public IEnumerable<Alternative> Alternatives => alternatives;

    /// <summary>
    /// Builds the graph of contracts.
    /// </summary>
    private DirectedAcyclicGraph<ContractId> BuildGraph()
    {
        var graph = new DirectedAcyclicGraph<ContractId>();
        foreach (var contractId in contracts.Keys)
        {
            graph.AddVertex(contractId);
        }

        foreach (var (contractId, contract) in contracts)
        {
            foreach (var dependency in contract.DependsOn)
            {
                graph.AddEdge(dependency, contractId);
            }
        }

        return graph;
    }

    /// <summary>
    /// Get contract by id.
    /// </summary>
    public Contract GetContract(ContractId contractId)
    {
        return contracts[contractId];
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
    public Application Install()
    {
        var graph = BuildGraph();
        var installedContracts = new Dictionary<ContractId, Contract>();
        graph.RunDfs(contractId =>
            {
                var contract = GetContract(contractId);

                foreach (var argument in contract.GetArguments())
                {
                    argument.Resolve(this);
                }

                var installedContract = contract.Install(this);

                if (installedContract is not Application)
                {
                    installedContracts[contractId] = installedContract;
                }

                contracts[contractId] = installedContract;

                var handlerApplication = installedContract.Handler?.Application;
                if (handlerApplication != null)
                {
                    _requiredApplications.Add(handlerApplication);
                }
            }
        );

        return CreateApplication(installedContracts);
    }

    /// <summary>
    /// Creates an application from the installed contracts.
    /// </summary>
    private Application CreateApplication(Dictionary<ContractId, Contract> installedContracts)
    {
        var application = contracts.Values.OfType<Application>().First();
        
        return application with
        {
            Contracts = new ContractList(installedContracts),
            RequiredApplications = _requiredApplications.Select(app => app.Name).ToList()
        };
    }
}