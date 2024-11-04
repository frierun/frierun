using Docker.DotNet.Models;
using Frierun.Server.Models;
using Frierun.Server.Resources;
using Frierun.Server.Services;

namespace Frierun.Server.Providers;

public class VolumeProvider(DockerService dockerService) : Provider<Volume, VolumeDefinition>
{
    /// <inheritdoc />
    protected override void FillParameters(ExecutionPlan<Volume, VolumeDefinition> plan)
    {
        var defaultName = plan.Definition.Name ?? "";
        plan.Parameters["name"] = defaultName;

        var count = 1;
        while (!Validate(plan))
        {
            count++;
            plan.Parameters["name"] = $"{defaultName}{count}";
        }
    }

    /// <inheritdoc />
    protected override bool Validate(ExecutionPlan<Volume, VolumeDefinition> plan)
    {
        var name = plan.GetFullName();
        return plan.State.Resources.OfType<Volume>().All(resource => resource.Name != name);
    }

    /// <inheritdoc />
    protected override Volume Install(ExecutionPlan<Volume, VolumeDefinition> plan)
    {
        var name = plan.GetFullName();

        if (plan.Parent is not ContainerExecutionPlan parentPlan)
        {
            throw new Exception("Parent plan must be a container");
        }

        parentPlan.StartContainer += parameters =>
        {
            parameters.HostConfig.Mounts.Add(new Mount
                {
                    Source = name,
                    Target = plan.Definition.Path,
                    Type = "volume",
                    ReadOnly = plan.Definition.ReadOnly
                }
            );
        };

        return new Volume(name)
        {
            DependsOn = plan.InstallChildren()
        };
    }

    /// <inheritdoc />
    protected override void Uninstall(Volume resource)
    {
        dockerService.RemoveVolume(resource.Name).Wait();
    }
}