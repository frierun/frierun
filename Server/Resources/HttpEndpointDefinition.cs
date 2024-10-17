namespace Frierun.Server.Resources;

public record HttpEndpointDefinition(int Port) : ResourceDefinition<HttpEndpoint>;
