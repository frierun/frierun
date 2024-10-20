using Docker.DotNet.Models;
using Frierun.Server.Models;
using Frierun.Server.Resources;

namespace Frierun.Server.Providers;

public class VolumeProvider : Provider<Volume, VolumeDefinition>, IChangesContainer
{
    /// <inheritdoc />
    protected override void FillParameters(ExecutionPlan<Volume, VolumeDefinition> plan)
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
    protected override bool Validate(ExecutionPlan<Volume, VolumeDefinition> plan)
    {
        if (!plan.Parameters.TryGetValue("name", out var name))
        {
            return false;
        }

        return plan.State.Resources.OfType<Volume>().All(resource => resource.Name != name);
    }

    /// <inheritdoc />
    protected override Volume Install(ExecutionPlan<Volume, VolumeDefinition> plan)
    {
        return new Volume(Guid.NewGuid(), plan.Parameters["name"]);
    }

    /// <inheritdoc />
    public void ChangeContainer(ExecutionPlan plan, CreateContainerParameters parameters)
    {
        var volumePlan = (ExecutionPlan<Volume, VolumeDefinition>)plan;
        var name = volumePlan.Parameters["name"];
        
        parameters.HostConfig.Mounts.Add(new Mount
            {
                Source = name,
                Target = volumePlan.Definition.Path,
                Type = "volume"
            }
        );
    }
}