using Docker.DotNet.Models;
using Frierun.Server.Models;
using Frierun.Server.Resources;

namespace Frierun.Server.Providers;

public class VolumeProvider : Provider<Volume, VolumeDefinition>
{
    /// <inheritdoc />
    protected override IDictionary<string, string> GetParameters(ExecutionPlan plan, VolumeDefinition definition)
    {
        var containerDefinition = plan.ResourcesToInstall.OfType<ContainerDefinition>().First();
        var containerParameters = plan.Parameters[containerDefinition];
        var containerName = containerParameters["name"];
        
        var result = new Dictionary<string, string>
        {
            ["name"] = $"{containerName}-{definition.Name}"
        };

        var count = 1;
        while (!Validate(plan, result))
        {
            count++;
            result["name"] = $"{containerName}-{definition.Name}{count}";
        }
        
        return result;
    }

    /// <inheritdoc />
    public override bool Validate(ExecutionPlan plan, IDictionary<string, string> parameters)
    {
        if (!parameters.TryGetValue("name", out var name))
        {
            return false;
        }

        return plan.State.Resources.OfType<Volume>().All(resource => resource.Name != name);
    }

    /// <inheritdoc />
    protected override Volume Create(ExecutionPlan plan,
        IDictionary<string, string> parameters,
        VolumeDefinition definition)
    {
        var name = parameters["name"];

        plan.ContainerParameters.Add(containerParameters =>
            {
                containerParameters.HostConfig.Mounts.Add(new Mount
                    {
                        Source = name,
                        Target = definition.Path,
                        Type = "volume"
                    }
                );
            }
        );

        return new Volume(Guid.NewGuid(), name);
    }
}