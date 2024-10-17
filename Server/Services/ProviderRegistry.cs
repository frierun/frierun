using Frierun.Server.Providers;
using Frierun.Server.Resources;

namespace Frierun.Server.Services;

public class ProviderRegistry
{
    private readonly Dictionary<Type, object> _providers = new();

    public ProviderRegistry(
        ContainerProvider containerProvider, 
        HttpEndpointProvider httpEndpointProvider,
        VolumeProvider volumeProvider)
    {
        _providers.Add(typeof(Container), containerProvider);
        _providers.Add(typeof(HttpEndpoint), httpEndpointProvider);
        _providers.Add(typeof(Volume), volumeProvider);
    }

    public Provider? Get(Type resourceType)
    {
        return _providers[resourceType] as Provider;
    }
}