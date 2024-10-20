using Frierun.Server.Models;
using Frierun.Server.Providers;
using Frierun.Server.Resources;

namespace Frierun.Server.Services;

public class ProviderRegistry
{
    private readonly Dictionary<Type, object> _providers = new();

    public ProviderRegistry(
        State state,
        ApplicationProvider applicationProvider,
        ContainerProvider containerProvider, 
        PortHttpEndpointProvider portHttpEndpointProvider,
        VolumeProvider volumeProvider,
        TraefikHttpEndpointProvider traefikHttpEndpointProvider
    )
    {
        _providers.Add(typeof(Application), applicationProvider);
        _providers.Add(typeof(Container), containerProvider);
        _providers.Add(typeof(HttpEndpoint), portHttpEndpointProvider);
        _providers.Add(typeof(TraefikHttpEndpointProvider), traefikHttpEndpointProvider);
        _providers.Add(typeof(Volume), volumeProvider);
        
        if (state.Applications.Any(a => a.Package?.Name == "traefik"))
        {
            UseTraefik();
        }
    }

    public void UseTraefik()
    {
        _providers[typeof(HttpEndpoint)] = _providers[typeof(TraefikHttpEndpointProvider)];
    }

    public Provider? Get(Type resourceType)
    {
        return _providers[resourceType] as Provider;
    }
}