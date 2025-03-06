using Frierun.Server.Data;
using Mount = Frierun.Server.Data.Mount;

namespace Frierun.Server.Installers.Docker;

public class MountInstaller : IInstaller<Mount>
{
    /// <inheritdoc />
    IEnumerable<InstallerInitializeResult> IInstaller<Mount>.Initialize(Mount contract, string prefix, State state)
    {
        yield return new InstallerInitializeResult(
            contract,
            [
                contract.ContainerId,
                contract.VolumeId
            ]
        );
    }

    /// <inheritdoc />
    IEnumerable<ContractDependency> IInstaller<Mount>.GetDependencies(Mount contract, ExecutionPlan plan)
    {
        yield return new ContractDependency(contract.Id, contract.ContainerId);
        yield return new ContractDependency(contract.VolumeId, contract.Id);
        // add dependency to volume so it would be added to resource dependencies
        yield return new ContractDependency(contract.VolumeId, contract.ContainerId);
    }

    /// <inheritdoc />
    Resource? IInstaller<Mount>.Install(Mount contract, ExecutionPlan plan)
    {
        var containerContract = plan.GetContract(contract.ContainerId);

        if (containerContract == null)
        {
            throw new Exception("Container not found");
        }

        var volume = plan.GetResource<DockerVolume>(contract.VolumeId);

        plan.UpdateContract(
            containerContract with
            {
                Configure = containerContract.Configure.Append(
                    parameters =>
                    {
                        parameters.HostConfig.Mounts.Add(
                            new global::Docker.DotNet.Models.Mount
                            {
                                Source = volume.Name,
                                Target = contract.Path,
                                Type = "volume",
                                ReadOnly = contract.ReadOnly
                            }
                        );
                    }
                ),
            }
        );

        return null;
    }
}