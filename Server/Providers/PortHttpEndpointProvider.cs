using Docker.DotNet.Models;
using Frierun.Server.Models;
using Frierun.Server.Resources;

namespace Frierun.Server.Providers;

public class PortHttpEndpointProvider : Provider<HttpEndpoint, HttpEndpointDefinition>, IChangesContainer
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
            
            if (count > 1000)
            {
                throw new Exception("Port not found");
            }
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
        
        if (intPort < 1 || intPort > 65535)
        {
            return false;
        }
        
        return plan.State.Resources.OfType<PortHttpEndpoint>().All(resource => resource.Port != intPort);
    }

    /// <inheritdoc />
    protected override HttpEndpoint Install(ExecutionPlan<HttpEndpoint, HttpEndpointDefinition> plan)
    {
        var port = int.Parse(plan.Parameters["port"]);
        // TODO: fill the correct ip of the host
        return new PortHttpEndpoint(Guid.NewGuid(), "127.0.0.1", port);
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