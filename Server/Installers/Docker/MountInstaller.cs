using Frierun.Server.Data;
using Mount = Frierun.Server.Data.Mount;

namespace Frierun.Server.Installers.Docker;

public class MountInstaller : IInstaller<Mount>
{
    /// <inheritdoc />
    public IEnumerable<ContractDependency> Dependencies(Mount contract, ExecutionPlan plan)
    {
        yield return new ContractDependency(
            contract,
            new Container(contract.ContainerName)
        );
        yield return new ContractDependency(
            new Volume(contract.VolumeName),
            contract
        );
        
        // add dependency to volume so it would be added to dependencies
        yield return new ContractDependency(
            new Volume(contract.VolumeName),
            new Container(contract.ContainerName)
        );
    }

    /// <inheritdoc />
    public Resource? Install(Mount contract, ExecutionPlan plan)
    {
        var containerContract = plan.GetContract<Container>(contract.ContainerId);

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