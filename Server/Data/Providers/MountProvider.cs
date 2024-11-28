using Docker.DotNet.Models;

namespace Frierun.Server.Data;

public class MountProvider : IInstaller<MountContract>
{
    /// <inheritdoc />
    public IEnumerable<ContractDependency> Dependencies(MountContract contract, ExecutionPlan plan)
    {
        yield return new ContractDependency(
            contract,
            new ContainerContract(contract.ContainerName)
        );
        yield return new ContractDependency(
            new VolumeContract(contract.VolumeName),
            contract
        );
    }

    /// <inheritdoc />
    public Contract Initialize(MountContract contract, ExecutionPlan plan)
    {
        return contract;
    }

    /// <inheritdoc />
    public Resource? Install(MountContract contract, ExecutionPlan plan)
    {
        var containerContract = plan.GetContract<ContainerContract>(contract.ContainerId);

        if (containerContract == null)
        {
            throw new Exception("Container not found");
        }

        var volume = plan.GetResource<Volume>(contract.VolumeId);

        plan.UpdateContract(
            containerContract with
            {
                Configure = containerContract.Configure.Append(
                    parameters =>
                    {
                        parameters.HostConfig.Mounts.Add(
                            new Docker.DotNet.Models.Mount
                            {
                                Source = volume.Name,
                                Target = contract.Path,
                                Type = "volume",
                                ReadOnly = contract.ReadOnly
                            }
                        );
                    }
                ),
                DependsOn = containerContract.DependsOn.Append(volume)
            }
        );
        
        return null;
    }
}