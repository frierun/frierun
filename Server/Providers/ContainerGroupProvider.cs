using Docker.DotNet.Models;
using Frierun.Server.Models;
using Frierun.Server.Resources;
using Frierun.Server.Services;

namespace Frierun.Server.Providers;

public class ContainerGroupProvider(DockerService dockerService) : Provider<ContainerGroup, ContainerGroupDefinition>
{
    /// <inheritdoc />
    protected override void FillParameters(ExecutionPlan<ContainerGroupDefinition> plan)
    {
        return;
    }

    /// <inheritdoc />
    protected override bool Validate(ExecutionPlan<ContainerGroupDefinition> plan)
    {
        return true;
    }

    /// <inheritdoc />
    protected override ContainerGroup Install(ExecutionPlan<ContainerGroupDefinition> plan)
    {
        var name = GetFullName(plan);

        dockerService.CreateNetwork(name).Wait();

        foreach (var selector in plan.Children)
        {
            if (selector.Selected is not ContainerExecutionPlan childPlan)
            {
                continue;
            }

            childPlan.StartContainer += parameters =>
            {
                parameters.Labels["com.docker.compose.project"] = name;
                parameters.Labels["com.docker.compose.service"] = childPlan.Definition.Name;

                parameters.NetworkingConfig.EndpointsConfig = new Dictionary<string, EndpointSettings>
                {
                    {
                        name, new EndpointSettings()
                        {
                            Aliases = new List<string> { childPlan.Definition.Name ?? "" }
                        }
                    }
                };
            };
        }

        var containers = plan.InstallChildren();

        return new ContainerGroup(Guid.NewGuid(), name, containers);
    }

    /// <inheritdoc />
    protected override void Uninstall(ContainerGroup resource)
    {
        dockerService.RemoveNetwork(resource.Name).Wait();
    }
}