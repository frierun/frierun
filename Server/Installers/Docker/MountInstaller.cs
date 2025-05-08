using Frierun.Server.Data;
using Mount = Frierun.Server.Data.Mount;

namespace Frierun.Server.Installers.Docker;

public class MountInstaller(Application application) : IInstaller<Mount>
{
    public Application Application => application;
    
    /// <inheritdoc />
    IEnumerable<InstallerInitializeResult> IInstaller<Mount>.Initialize(Mount contract, string prefix)
    {
        yield return new InstallerInitializeResult(
            contract with
            {
                DependsOn = contract.DependsOn.Append(contract.VolumeId),
                DependencyOf = contract.DependencyOf.Append(contract.ContainerId),
            }
        );
    }
    
    /// <inheritdoc />
    Resource? IInstaller<Mount>.Install(Mount contract, ExecutionPlan plan)
    {
        var containerContract = plan.GetContract(contract.ContainerId);

        if (containerContract == null)
        {
            throw new Exception("Container not found");
        }

        var volume = plan.GetResource(contract.VolumeId);

        if (volume is DockerVolume dockerVolume)
        {
            plan.UpdateContract(
                containerContract with
                {
                    Configure = containerContract.Configure.Append(
                        parameters =>
                        {
                            parameters.HostConfig.Mounts.Add(
                                new global::Docker.DotNet.Models.Mount
                                {
                                    Source = dockerVolume.Name,
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

        if (volume is LocalPath localPath)
        {
            plan.UpdateContract(
                containerContract with
                {
                    Configure = containerContract.Configure.Append(
                        parameters =>
                        {
                            parameters.HostConfig.Mounts.Add(
                                new global::Docker.DotNet.Models.Mount
                                {
                                    Source = localPath.Path,
                                    Target = contract.Path,
                                    Type = "bind",
                                    ReadOnly = contract.ReadOnly
                                }
                            );
                        }
                    ),
                }
            );

            return null;
        }
        
        throw new Exception("Unknown volume type: " + volume?.GetType().Name);
    }
}