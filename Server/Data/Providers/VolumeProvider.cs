using Docker.DotNet.Models;
using Frierun.Server.Services;

namespace Frierun.Server.Data;

public class VolumeProvider(DockerService dockerService) : Provider<Volume, VolumeContract>
{
    /// <inheritdoc />
    protected override Volume Install(VolumeContract contract, ExecutionPlan plan)
    {
        var volumeName = plan.GetPrefixedName(contract.Name);
        dockerService.CreateVolume(volumeName).Wait();
        return new Volume(volumeName);
    }

    /// <inheritdoc />
    protected override void Uninstall(Volume resource)
    {
        dockerService.RemoveVolume(resource.Name).Wait();
    }
}