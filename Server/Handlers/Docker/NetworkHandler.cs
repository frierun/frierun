using System.Diagnostics;
using Frierun.Server.Data;
using Network = Frierun.Server.Data.Network;

namespace Frierun.Server.Handlers.Docker;

public class NetworkHandler(State state, Application application, DockerService dockerService)
    : Handler<Network>(state, application)
{
    public override IEnumerable<Network> Discover()
    {
        return dockerService.ListNetworks().Result
            .Where(network => network.Driver == "bridge")
            .Select(network => new Network()
                {
                    NetworkName = network.Name
                }
            );
    }

    public override IEnumerable<ContractList> Initialize(Network contract, ApplicationContext context)
    {
        if (contract.NetworkName != null)
        {
            var installedContract = State.GetContracts<Network>()
                .FirstOrDefault(network => network.NetworkName == contract.NetworkName && network.Handler == this);
            if (installedContract != null)
            {
                yield return new ContractList { [context] = installedContract };
            }
            else
            {
                yield return new ContractList { [context] = new Network { Handler = this } };
            }

            yield break;
        }

        var defaultName = context.Prefix + (context.Name == "" ? "" : $"-{context.Name}");
        var defaultNetwork = State.GetContracts<Network>()
            .FirstOrDefault(network => network.NetworkName == defaultName && network.Handler == this);

        // return the same network if it exists first
        if (defaultNetwork != null)
        {
            yield return new ContractList { [context] = defaultNetwork };
        }

        // return a new network
        yield return new ContractList
        {
            [context] = contract with
            {
                Handler = this,
                NetworkName = FindUniqueName(
                    context.Prefix + (context.Name == "" ? "" : $"-{context.Name}"),
                    c => c.NetworkName
                )
            }
        };

        // return all other installed networks
        foreach (var installedContract in State.GetContracts<Network>().Where(network => network.Handler == this))
        {
            if (installedContract.NetworkName == defaultName)
            {
                continue;
            }

            yield return new ContractList { [context] = installedContract };
        }
    }

    public override Network Install(Network contract, ExecutionPlan plan)
    {
        var networkName = contract.NetworkName;
        Debug.Assert(networkName != null, "Network name cannot be null.");

        dockerService.CreateNetwork(networkName).Wait();

        return contract;
    }

    public override void Uninstall(Network contract)
    {
        Debug.Assert(contract.Installed);
        dockerService.RemoveNetwork(contract.NetworkName).Wait();
    }
}