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
        var volume = plan.GetContract(contract.Volume);

        if (volume.VolumeName != null)
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
                                    Source = volume.VolumeName,
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

        if (volume.LocalPath != null)
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
                                    Source = volume.LocalPath,
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
        
        throw new Exception($"Unknown volume type for volume {volume.Name}");
    }
}