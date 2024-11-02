using Docker.DotNet.Models;
using Frierun.Server.Models;
using Frierun.Server.Resources;
using Frierun.Server.Services;

namespace Frierun.Server.Providers;

public class ContainerProvider(DockerService dockerService)
    : Provider<Container, ContainerDefinition, ContainerExecutionPlan>
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
    protected override bool Validate(ContainerExecutionPlan plan)
    {
        var name = plan.GetFullName();
        return plan.State.Resources.OfType<Container>().All(resource => resource.Name != name);
    }

    /// <inheritdoc />
    protected override Container Install(ContainerExecutionPlan plan)
    {
        var name = plan.GetFullName();

        var dockerParameters = new CreateContainerParameters
        {
            Cmd = plan.Definition.Command.ToList(),
            Env = plan.Definition.Env.Select(kv => $"{kv.Key}={kv.Value}").ToList(),
            Image = plan.Definition.ImageName,
            HostConfig = new HostConfig
            {
                Mounts = new List<Mount>(),
                PortBindings = new Dictionary<string, IList<PortBinding>>()
            },
            Labels = new Dictionary<string, string>(),
            Name = name,
            NetworkingConfig = new NetworkingConfig()
            {
                EndpointsConfig = new Dictionary<string, EndpointSettings>()
            }
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

        return new Container(name)
        {
            DependsOn = children
        };
    }

    /// <inheritdoc />
    protected override void Uninstall(Container resource)
    {
        dockerService.StopContainer(resource.Name).Wait();
    }
}