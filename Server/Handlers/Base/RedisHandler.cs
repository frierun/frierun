using System.Diagnostics;
using Frierun.Server.Data;

namespace Frierun.Server.Handlers.Base;

public class RedisHandler : IHandler<Redis>
{
    public IEnumerable<ContractInitializeResult> Initialize(Redis contract, string prefix)
    {
        yield return new ContractInitializeResult(
            contract with
            {
                Handler = this,
                DependsOn = contract.DependsOn.Append(contract.ContainerId)
            },
            [
                new Container(
                    Name: contract.ContainerName,
                    ImageName: "redis:7",
                    NetworkName: contract.NetworkName,
                    ContainerName: contract.Host
                ),
                new Mount(
                    Path: "/data",
                    VolumeName: contract.ContainerName + "-data",
                    ContainerName: contract.ContainerName
                )
            ]
        );
    }

    public Redis Install(Redis contract, ExecutionPlan plan)
    {
        var container = plan.GetContract(contract.ContainerId);
        Debug.Assert(container.Installed);
        Debug.Assert(contract.Host == null || contract.Host == container.ContainerName);
        
        return contract with
        {
            Host = container.ContainerName
        };
    }
}