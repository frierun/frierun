using Docker.DotNet.Models;
using Frierun.Server.Models;
using Frierun.Server.Providers;
using Frierun.Server.Resources;
using File = Frierun.Server.Resources.File;

namespace Frierun.Server.Services;

public class ProviderRegistry
{
    private readonly DockerService _dockerService;
    private readonly Dictionary<Type, IList<Provider>> _providers = new();

    public ProviderRegistry(
        State state,
        ApplicationProvider applicationProvider,
        ContainerGroupProvider containerGroupProvider,
        ContainerProvider containerProvider,
        FileProvider fileProvider,
        PortHttpEndpointProvider portHttpEndpointProvider,
        VolumeProvider volumeProvider,
        DockerService dockerService
    )
    {
        _dockerService = dockerService;
        Add(typeof(Application), applicationProvider);
        Add(typeof(ContainerGroup), containerGroupProvider);
        Add(typeof(Container), containerProvider);
        Add(typeof(File), fileProvider);
        Add(typeof(HttpEndpoint), portHttpEndpointProvider);
        Add(typeof(Volume), volumeProvider);
        
        var traefik = state.Applications.FirstOrDefault(a => a.Package?.Name == "traefik");
        if (traefik != null)
        {
            UseTraefik(traefik);
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

    public void UseTraefik(Application application)
    {
        var traefikHttpEndpointProvider = new TraefikHttpEndpointProvider(_dockerService, application);
        Add(typeof(HttpEndpoint), traefikHttpEndpointProvider);
        Add(typeof(TraefikHttpEndpoint), traefikHttpEndpointProvider);
    }

    public IList<Provider> Get(Type resourceType)
    {
        return _providers[resourceType];
    }
}