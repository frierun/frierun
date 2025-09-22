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
        if (guid != null)
        {
            return contracts.Values.First(contract => contract.Id == guid);
        }
        
        Debug.Assert(contractId.Ref != null, "Contract ID must have either a GUID or a ContractRef");;
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
    public Application Install()
    {
        var graph = BuildGraph();
        var installedContracts = new Dictionary<ContractRef, Contract>();
        graph.RunDfs(contractRef =>
            {
                var contract = GetContract(contractRef);

                foreach (var argument in contract.GetArguments())
                {
                    argument.Resolve(this);
                }

                var installedContract = contract.Install(this);

                if (installedContract is not Application)
                {
                    installedContracts[contractRef] = installedContract;
                }

                contracts[contractRef] = installedContract;

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
    private Application CreateApplication(Dictionary<ContractRef, Contract> installedContracts)
    {
        var application = contracts.Values.OfType<Application>().First();

        return application with
        {
            Contracts = new ContractList(installedContracts),
            RequiredApplications = _requiredApplications.Select(app => app.Name).ToList(),
            ContractRefs = installedContracts.ToDictionary(
                pair => pair.Key, pair => pair.Value.Id ?? throw new Exception($"Contract {pair.Key} is not installed")
            ),
        };
    }
}