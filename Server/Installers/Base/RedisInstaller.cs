using Frierun.Server.Data;

namespace Frierun.Server.Installers.Base;

public class RedisInstaller : IInstaller<Redis>
{
    public Application? Application => null;

    IEnumerable<InstallerInitializeResult> IInstaller<Redis>.Initialize(Redis contract, string prefix)
    {
        yield return new InstallerInitializeResult(
            contract with
            {
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

    Redis IInstaller<Redis>.Install(Redis contract, ExecutionPlan plan)
    {
        var container = plan.GetResource<DockerContainer>(contract.ContainerId);

        return contract with
        {
            Result = new RedisDatabase(new EmptyHandler()) { Host = container.Name }
        };
    }
}