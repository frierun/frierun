using Docker.DotNet.Models;
using Frierun.Server.Models;
using Frierun.Server.Resources;

namespace Frierun.Server.Providers;

public class VolumeProvider : Provider<Volume, VolumeDefinition>
{
    /// <inheritdoc />
    protected override void FillParameters(ExecutionPlan<VolumeDefinition> plan)
    {
        var containerName = plan.Parent?.Parameters["name"];
        if (containerName == null)
        {
            throw new InvalidOperationException("Volume must be created as a child of a container");
        }
        
        plan.Parameters["name"] = $"{containerName}-{plan.Definition.Name}";

        var count = 1;
        while (!Validate(plan))
        {
            count++;
            plan.Parameters["name"] = $"{containerName}-{plan.Definition.Name}{count}";
        }
    }

    /// <inheritdoc />
    protected override bool Validate(ExecutionPlan<VolumeDefinition> plan)
    {
        if (!plan.Parameters.TryGetValue("name", out var name))
        {
            return false;
        }

        return plan.State.Resources.OfType<Volume>().All(resource => resource.Name != name);
    }

    /// <inheritdoc />
    protected override Volume Install(ExecutionPlan<VolumeDefinition> plan)
    {
        var name = plan.Parameters["name"];

        var parentPlan = plan.Parent as ContainerExecutionPlan;
        if (parentPlan == null)
        {
            throw new Exception("Parent plan must be a container");
        }

        parentPlan.StartContainer += (parameters) =>
        {
            parameters.HostConfig.Mounts.Add(new Mount
                {
                    Source = name,
                    Target = plan.Definition.Path,
                    Type = "volume"
                }
            );
        };

        return new Volume(Guid.NewGuid(), name);
    }
}