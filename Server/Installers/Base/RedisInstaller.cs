using Frierun.Server.Data;

namespace Frierun.Server.Installers.Base;

public class RedisInstaller : IInstaller<Redis>, IUninstaller<RedisDatabase>
{
    /// <inheritdoc />
    IEnumerable<InstallerInitializeResult> IInstaller<Redis>.Initialize(Redis contract, string prefix)
    {
        yield return new InstallerInitializeResult(
            contract,
            [],
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

    /// <inheritdoc />
    IEnumerable<ContractDependency> IInstaller<Redis>.GetDependencies(Redis contract, ExecutionPlan plan)
    {
        yield return new ContractDependency(contract.ContainerId, contract);
    }

    /// <inheritdoc />
    Resource? IInstaller<Redis>.Install(Redis contract, ExecutionPlan plan)
    {
        var container = plan.GetResource<DockerContainer>(contract.ContainerId);
        
        return new RedisDatabase(container.Name)
        {
            DependsOn = [container]
        };
    }
}