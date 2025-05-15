using Frierun.Server.Data;

namespace Frierun.Server.Installers.Base;

public class RedisInstaller : IHandler<Redis>
{
    public IEnumerable<InstallerInitializeResult> Initialize(Redis contract, string prefix)
    {
        yield return new InstallerInitializeResult(
            contract with
            {
                Handler = this,
                DependsOn = contract.DependsOn.Append(contract.ContainerId)
            },
            [
                new Container(
                    Name: contract.ContainerName,
                    ImageName: "redis:7",
                    NetworkName: contract.NetworkName
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
        var container = plan.GetResource<DockerContainer>(contract.ContainerId);

        return contract with
        {
            Result = new RedisDatabase { Host = container.Name }
        };
    }
}