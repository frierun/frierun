using Frierun.Server.Models;
using Frierun.Server.Resources;
using Frierun.Server.Services;

namespace Frierun.Server.Providers;

public class TraefikHttpEndpointProvider(DockerService dockerService, Application application) : Provider<HttpEndpoint, HttpEndpointDefinition>
{
    /// <inheritdoc />
    protected override void FillParameters(ExecutionPlan<HttpEndpoint, HttpEndpointDefinition> plan)
    {
        var name = plan.Parent?.GetFullName();
        
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

        if (plan.Parent is not ContainerExecutionPlan containerPlan)
        {
            throw new Exception("Parent plan must be a container");
        }
        
        var traefikContainer = application.AllDependencies.OfType<Container>().FirstOrDefault();
        if (traefikContainer == null)
        {
            throw new Exception("Traefik container not found");
        }
        
        containerPlan.StartContainer += parameters =>
        {
            var subdomain = domain.Split('.')[0];
            
            parameters.Labels["traefik.enable"] = "true";
            parameters.Labels["traefik.http.routers." + subdomain + ".rule"] = "Host(`" + domain + "`)";
            parameters.Labels["traefik.http.services." + subdomain + ".loadbalancer.server.port"] = plan.Definition.Port.ToString();
            
        };

        var traefikPort = application.AllDependencies.OfType<PortHttpEndpoint>().FirstOrDefault();
        if (traefikPort == null)
        {
            throw new Exception("Traefik port not found");
        }
        
        var dependsOn = new List<Resource>
        {
            application,
        };
        
        var containerGroupPlan = containerPlan.Parent as ExecutionPlan<ContainerGroup, ContainerGroupDefinition>;
        var containerGroup = containerGroupPlan?.Install();
        if (containerGroup != null)
        {
            dependsOn.Add(containerGroup);
            
            dockerService.AttachNetwork(containerGroup.Name, traefikContainer.Name).Wait();
        }
        
        return new TraefikHttpEndpoint(domain, traefikPort.Port){
            DependsOn = dependsOn
        };
    }

    /// <inheritdoc />
    protected override void Uninstall(HttpEndpoint resource)
    {
        var containerGroup = resource.DependsOn.OfType<ContainerGroup>().FirstOrDefault();
        if (containerGroup == null)
        {
            return;
        }
        
        var traefikContainer = application.AllDependencies.OfType<Container>().FirstOrDefault();
        if (traefikContainer == null)
        {
            throw new Exception("Traefik container not found");
        }

        dockerService.DetachNetwork(containerGroup.Name, traefikContainer.Name).Wait();
    }
}