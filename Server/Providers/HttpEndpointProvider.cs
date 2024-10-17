using Docker.DotNet.Models;
using Frierun.Server.Models;
using Frierun.Server.Resources;

namespace Frierun.Server.Providers;

public class HttpEndpointProvider : Provider<HttpEndpoint, HttpEndpointDefinition>
{
    /// <inheritdoc />
    protected override IDictionary<string, string> GetParameters(ExecutionPlan plan, HttpEndpointDefinition definition)
    {
        var result = new Dictionary<string, string>
        {
            ["port"] = "80"
        };
        
        var count = 0;
        while (!Validate(plan, result))
        {
            result["port"] = (8080 + count).ToString();
            count++;
        }

        return result;
    }

    /// <inheritdoc />
    public override bool Validate(ExecutionPlan plan, IDictionary<string, string> parameters)
    {
        if (!parameters.TryGetValue("port", out var port))
        {
            return false;
        }

        var intPort = int.Parse(port);
        return plan.State.Resources.OfType<HttpEndpoint>().All(resource => resource.Port != intPort);
    }

    /// <inheritdoc />
    protected override HttpEndpoint Create(ExecutionPlan plan,
        IDictionary<string, string> parameters,
        HttpEndpointDefinition definition)
    {
        var port = int.Parse(parameters["port"]);

        plan.ContainerParameters.Add(containerParameters =>
                containerParameters.HostConfig.PortBindings[$"{definition.Port}/tcp"] =
                    new List<PortBinding>
                    {
                        new()
                        {
                            HostPort = port.ToString()
                        }
                    }
            );

        return new HttpEndpoint(Guid.NewGuid(), port);
    }
}