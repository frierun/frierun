using System.Diagnostics;

namespace Frierun.Server.Data;

public class ExecutionPlan(Dictionary<ContractId, Contract> contracts, IEnumerable<Contract> alternatives) : IExecutionPlan
{
    private readonly HashSet<Application> _requiredApplications = [];

    public IEnumerable<Contract> Contracts => contracts.Values;
    
    /// <summary>
    /// List of all contracts that are alternatives to the main execution plan.
    /// </summary>
    public IEnumerable<Contract> Alternatives => alternatives;

    /// <summary>
    /// Builds the graph of contracts.
    /// </summary>
    private DirectedAcyclicGraph<ContractId> BuildGraph()
    {
        var graph = new DirectedAcyclicGraph<ContractId>();
        foreach (var contract in contracts.Values)
        {
            graph.AddVertex(contract.Id);
        }

        foreach (var contract in contracts.Values)
        {
            foreach (var dependency in contract.DependsOn)
            {
                graph.AddEdge(dependency, contract);
            }

            foreach (var dependency in contract.DependencyOf)
            {
                graph.AddEdge(contract, dependency);
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
    /// Replaces contract with another one
    /// </summary>
    public void ReplaceContract(Contract contract)
    {
        Debug.Assert(
            !contracts.TryGetValue(contract, out var existing) || !existing.Installed,
            $"Contract is already installed"
        );

        contracts[contract] = contract;
    }

    /// <summary>
    /// Installs all contracts in the execution plan.
    /// </summary>
    public Application Install()
    {
        var graph = BuildGraph();
        var installedContracts = new List<Contract>();
        graph.RunDfs(
            contractId =>
            {
                var contract = GetContract(contractId);
                Debug.Assert(!contract.Installed);

                var installedContract = contract.Install(this);

                Debug.Assert(installedContract.Id == contractId);

                if (installedContract is not Package)
                {
                    installedContracts.Add(installedContract);
                }

                contracts[contractId] = installedContract;

                var handlerApplication = installedContract.Handler?.Application;
                if (handlerApplication != null)
                {
                    _requiredApplications.Add(handlerApplication);
                }
            }
        );

        var application = contracts.Values.OfType<Package>().First().Result;
        Debug.Assert(application != null);

        return new Application
        {
            Name = application.Name,
            Package = application.Package,
            Description = application.Description,
            Url = application.Url,
            Contracts = installedContracts,
            RequiredApplications = _requiredApplications.Select(app => app.Name).ToList()
        };
    }
}