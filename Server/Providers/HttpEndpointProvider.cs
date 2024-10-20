using Docker.DotNet.Models;
using Frierun.Server.Models;
using Frierun.Server.Resources;

namespace Frierun.Server.Providers;

public class HttpEndpointProvider : Provider<HttpEndpoint, HttpEndpointDefinition>, IChangesContainer
{
    /// <inheritdoc />
    protected override void FillParameters(ExecutionPlan<HttpEndpoint, HttpEndpointDefinition> plan)
    {
        plan.Parameters["port"] = "80";
        
        var count = 0;
        while (!Validate(plan))
        {
            plan.Parameters["port"] = (8080 + count).ToString();
            count++;
        }
    }

    /// <inheritdoc />
    protected override bool Validate(ExecutionPlan<HttpEndpoint, HttpEndpointDefinition> plan)
    {
        if (!plan.Parameters.TryGetValue("port", out var port))
        {
            return false;
        }

        var intPort = int.Parse(port);
        return plan.State.Resources.OfType<HttpEndpoint>().All(resource => resource.Port != intPort);
    }

    /// <inheritdoc />
    protected override HttpEndpoint Install(ExecutionPlan<HttpEndpoint, HttpEndpointDefinition> plan)
    {
        var port = int.Parse(plan.Parameters["port"]);
        return new HttpEndpoint(Guid.NewGuid(), port);
    }
    
    /// <inheritdoc />
    public void ChangeContainer(ExecutionPlan plan, CreateContainerParameters parameters)
    {
        var httpEndpointPlan = (ExecutionPlan<HttpEndpoint, HttpEndpointDefinition>)plan;
        var port = int.Parse(httpEndpointPlan.Parameters["port"]);
        
        parameters.HostConfig.PortBindings[$"{httpEndpointPlan.Definition.Port}/tcp"] =
            new List<PortBinding>
            {
                new()
                {
                    HostPort = port.ToString()
                }
            };
    }    
}