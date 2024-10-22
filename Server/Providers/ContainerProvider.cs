using Docker.DotNet.Models;
using Frierun.Server.Models;
using Frierun.Server.Resources;
using Frierun.Server.Services;

namespace Frierun.Server.Providers;

public class ContainerProvider(DockerService dockerService) : Provider<Container, ContainerDefinition, ContainerExecutionPlan>
{
    /// <inheritdoc />
    public override ExecutionPlan CreatePlan(State state, ResourceDefinition definition, ExecutionPlan? parent)
    {
        var plan = new ContainerExecutionPlan(state, (ContainerDefinition)definition, this, parent);
        FillParameters(plan);
        return plan;
    }

    /// <inheritdoc />
    protected override void FillParameters(ContainerExecutionPlan plan)
    {
        var applicationName = plan.Parent?.Parameters["name"];
        
        if (applicationName == null)
        {
            throw new Exception("Application name not found");
        }
        
        plan.Parameters["name"] = applicationName;

        var count = 1;
        while (!Validate(plan))
        {
            count++;
            plan.Parameters["name"] = $"{applicationName}{count}";
        }
    }

    /// <inheritdoc />
    protected override bool Validate(ContainerExecutionPlan plan)
    {
        if (!plan.Parameters.TryGetValue("name", out var name))
        {
            return false;
        }
        return plan.State.Resources.OfType<Container>().All(resource => resource.Name != name);
    }

    /// <inheritdoc />
    protected override Container Install(ContainerExecutionPlan plan)
    {
        var name = plan.Parameters["name"];
        
        var dockerParameters = new CreateContainerParameters
        {
            Cmd = plan.Definition.Command?.Split(' '),
            Image = plan.Definition.ImageName,
            HostConfig = new HostConfig
            {
                PortBindings = new Dictionary<string, IList<PortBinding>>(),
                Mounts = new List<Mount>()
            },
            Labels = new Dictionary<string, string>(),
            Name = name,
        };
        
        if (plan.Definition.RequireDocker)
        {
            dockerParameters.HostConfig.Mounts.Add(new Mount
            {
                Source = "/var/run/docker.sock",
                Target = "/var/run/docker.sock",
                Type = "bind"
            });
        }

        var children = plan.InstallChildren();

        plan.OnStartContainer(dockerParameters);

        var result = dockerService.StartContainer(dockerParameters).Result;

        if (!result)
        {
            throw new Exception("Failed to start container");
        }

        return new Container(Guid.NewGuid(), name, children);

    }
}