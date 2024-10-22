using Docker.DotNet.Models;
using Frierun.Server.Models;
using Frierun.Server.Providers;
using Frierun.Server.Resources;
using File = Frierun.Server.Resources.File;

namespace Frierun.Server.Services;

public class ProviderRegistry
{
    private readonly Dictionary<Type, IList<Provider>> _providers = new();

    public ProviderRegistry(
        State state,
        ApplicationProvider applicationProvider,
        ContainerProvider containerProvider,
        FileProvider fileProvider,
        PortHttpEndpointProvider portHttpEndpointProvider,
        VolumeProvider volumeProvider,
        TraefikHttpEndpointProvider traefikHttpEndpointProvider
    )
    {
        Add(typeof(Application), applicationProvider);
        Add(typeof(Container), containerProvider);
        Add(typeof(File), fileProvider);
        Add(typeof(HttpEndpoint), portHttpEndpointProvider);
        Add(typeof(TraefikHttpEndpoint), traefikHttpEndpointProvider);
        Add(typeof(Volume), volumeProvider);
        
        if (state.Applications.Any(a => a.Package?.Name == "traefik"))
        {
            UseTraefik();
        }
    }

    private void Add(Type resourceType, Provider provider)
    {
        if (!_providers.ContainsKey(resourceType))
        {
            _providers[resourceType] = new List<Provider>();
        }
        
        // Add the provider to the beginning of the list so that it is used first
        _providers[resourceType].Insert(0, provider);
    }

    public void UseTraefik()
    {
        var traefikHttpEndpointProvider = Get(typeof(TraefikHttpEndpoint)).FirstOrDefault();
        if (traefikHttpEndpointProvider == null)
        {
            throw new Exception("TraefikHttpEndpointProvider not found");
        }
        Add(typeof(HttpEndpoint), traefikHttpEndpointProvider);
    }

    public IList<Provider> Get(Type resourceType)
    {
        return _providers[resourceType];
    }
}