namespace Frierun.Server.Resources;

public record HttpEndpointDefinition(
    int Port,
    string? Name = null
) : ResourceDefinition<HttpEndpoint>(new List<ResourceDefinition>(), Name);