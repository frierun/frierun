using Docker.DotNet.Models;
using Frierun.Server.Models;
using Frierun.Server.Resources;
using Frierun.Server.Services;

namespace Frierun.Server.Providers;

public class ContainerProvider(DockerService dockerService) : Provider<Container, ContainerDefinition>
{
    /// <inheritdoc />
    protected override void FillParameters(ExecutionPlan<Container, ContainerDefinition> plan)
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
    protected override bool Validate(ExecutionPlan<Container, ContainerDefinition> plan)
    {
        if (!plan.Parameters.TryGetValue("name", out var name))
        {
            return false;
        }
        return plan.State.Resources.OfType<Container>().All(resource => resource.Name != name);
    }

    /// <inheritdoc />
    protected override Container Install(ExecutionPlan<Container, ContainerDefinition> plan)
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

        var children = new List<Resource>();
        foreach (var childPlan in plan.Children)
        {
            if (childPlan.Provider is IChangesContainer changesContainer)
            {
                changesContainer.ChangeContainer(childPlan, dockerParameters);
            }

            children.Add(childPlan.Install());
        }

        var result = dockerService.StartContainer(dockerParameters).Result;

        if (!result)
        {
            throw new Exception("Failed to start container");
        }

        return new Container(Guid.NewGuid(), name, children);

    }
}