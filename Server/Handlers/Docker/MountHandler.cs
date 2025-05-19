using Frierun.Server.Data;
using Mount = Frierun.Server.Data.Mount;

namespace Frierun.Server.Handlers.Docker;

public class MountHandler(Application application) : IHandler<Mount>
{
    public Application Application => application;
    
    public IEnumerable<ContractInitializeResult> Initialize(Mount contract, string prefix)
    {
        yield return new ContractInitializeResult(
            contract with
            {
                Handler = this,
                DependsOn = contract.DependsOn.Append(contract.Volume),
                DependencyOf = contract.DependencyOf.Append(contract.Container),
            }
        );
    }
    
    public Mount Install(Mount contract, ExecutionPlan plan)
    {
        var containerContract = plan.GetContract(contract.Container);
        var volume = plan.GetResource(contract.Volume);

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
                    )
                }
            );

            return contract;
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

            return contract;
        }
        
        throw new Exception("Unknown volume type: " + volume.GetType().Name);
    }
}