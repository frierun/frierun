using Frierun.Server.Models;
using Frierun.Server.Resources;

namespace Frierun.Server.Providers;

public class TraefikHttpEndpointProvider : Provider<HttpEndpoint, HttpEndpointDefinition>
{
    /// <inheritdoc />
    protected override void FillParameters(ExecutionPlan<HttpEndpointDefinition> plan)
    {
        var name = plan.Parent?.Parameters["name"];
        
        if (name == null)
        {
            throw new Exception("Container name not found");
        }
        
        plan.Parameters["domain"] = $"{name}.localhost";

        var count = 1;
        while (!Validate(plan))
        {
            count++;
            plan.Parameters["domain"] = $"{name}{count}.localhost";
        }
    }

    /// <inheritdoc />
    protected override bool Validate(ExecutionPlan<HttpEndpointDefinition> plan)
    {
        if (!plan.Parameters.TryGetValue("domain", out var domain))
        {
            return false;
        }

        return plan.State.Resources.OfType<TraefikHttpEndpoint>().All(resource => resource.Domain != domain);
    }

    /// <inheritdoc />
    protected override HttpEndpoint Install(ExecutionPlan<HttpEndpointDefinition> plan)
    {
        var domain = plan.Parameters["domain"];
        
        var parentPlan = plan.Parent as ContainerExecutionPlan;
        if (parentPlan == null)
        {
            throw new Exception("Parent plan must be a container");
        }


        parentPlan.StartContainer += (parameters) =>
        {
            var subdomain = domain.Split('.')[0];
            
            parameters.Labels["traefik.enable"] = "true";
            parameters.Labels["traefik.http.routers." + subdomain + ".rule"] = "Host(`" + domain + "`)";
            parameters.Labels["traefik.http.services." + subdomain + ".loadbalancer.server.port"] = plan.Definition.Port.ToString();            
        };

        // TODO: fill the correct port of the host
        return new TraefikHttpEndpoint(Guid.NewGuid(), domain, 80);
    }
}