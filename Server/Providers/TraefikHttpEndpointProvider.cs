using Docker.DotNet.Models;
using Frierun.Server.Models;
using Frierun.Server.Resources;

namespace Frierun.Server.Providers;

public class TraefikHttpEndpointProvider : Provider<HttpEndpoint, HttpEndpointDefinition>, IChangesContainer
{
    /// <inheritdoc />
    protected override void FillParameters(ExecutionPlan<HttpEndpoint, HttpEndpointDefinition> plan)
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
    protected override bool Validate(ExecutionPlan<HttpEndpoint, HttpEndpointDefinition> plan)
    {
        if (!plan.Parameters.TryGetValue("domain", out var domain))
        {
            return false;
        }

        return plan.State.Resources.OfType<TraefikHttpEndpoint>().All(resource => resource.Domain != domain);
    }

    /// <inheritdoc />
    protected override HttpEndpoint Install(ExecutionPlan<HttpEndpoint, HttpEndpointDefinition> plan)
    {
        var domain = plan.Parameters["domain"];
        // TODO: fill the correct port of the host
        return new TraefikHttpEndpoint(Guid.NewGuid(), domain, 80);
    }

    /// <inheritdoc />
    public void ChangeContainer(ExecutionPlan plan, CreateContainerParameters parameters)
    {
        var httpEndpointPlan = (ExecutionPlan<HttpEndpoint, HttpEndpointDefinition>)plan;
        var domain = httpEndpointPlan.Parameters["domain"];
        var subdomain = domain.Split('.')[0];
        
        parameters.Labels["traefik.enable"] = "true";
        parameters.Labels["traefik.http.routers." + subdomain + ".rule"] = "Host(`" + domain + "`)";
        parameters.Labels["traefik.http.services." + subdomain + ".loadbalancer.server.port"] = httpEndpointPlan.Definition.Port.ToString();
    }
}