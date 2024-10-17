using Docker.DotNet.Models;
using Frierun.Server.Models;
using Frierun.Server.Resources;
using Frierun.Server.Services;

namespace Frierun.Server.Providers;

public class ContainerProvider(DockerService dockerService) : Provider<Container, ContainerDefinition>
{
    public delegate void ExtendContainerParameters(CreateContainerParameters parameters);

    /// <inheritdoc />
    protected override IDictionary<string, string> GetParameters(ExecutionPlan plan, ContainerDefinition definition)
    {
        var result = new Dictionary<string, string>()
        {
            ["name"] = plan.Name
        };

        var count = 1;
        while (!Validate(plan, result))
        {
            count++;
            result["name"] = $"{plan.Name}{count}";
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
        return plan.State.Resources.OfType<Container>().All(resource => resource.Name != name);
    }

    /// <inheritdoc />
    protected override Container Create(ExecutionPlan plan,
        IDictionary<string, string> parameters,
        ContainerDefinition definition)
    {
        var name = parameters["name"];
        
        var dockerParameters = new CreateContainerParameters
        {
            Image = definition.ImageName,
            Name = name,
            Cmd = definition.Command?.Split(' '),
            HostConfig = new HostConfig
            {
                PortBindings = new Dictionary<string, IList<PortBinding>>(),
                Mounts = new List<Mount>()
            },
        };
        
        if (definition.RequireDocker)
        {
            dockerParameters.HostConfig.Mounts.Add(new Mount
            {
                Source = "/var/run/docker.sock",
                Target = "/var/run/docker.sock",
                Type = "bind"
            });
        }

        foreach (var extendParameter in plan.ContainerParameters)
        {
            extendParameter(dockerParameters);
        }

        var result = dockerService.StartContainer(dockerParameters).Result;

        if (!result)
        {
            throw new Exception("Failed to start container");
        }

        return new Container(Guid.NewGuid(), name);

    }
}