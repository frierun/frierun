using Frierun.Server.Data;

namespace Frierun.Server.Services;

public class ProviderRegistry
{
    private readonly DockerService _dockerService;
    private readonly Dictionary<Type, IList<IInstaller>> _installers = new();
    private readonly Dictionary<Type, IUninstaller> _uninstallers = new();

    public ProviderRegistry(
        State state,
        ApplicationProvider applicationProvider,
        ContainerProvider containerProvider,
        FileProvider fileProvider,
        MountProvider mountProvider,
        NetworkProvider networkProvider,
        PortEndpointProvider portEndpointProvider,
        PortHttpEndpointProvider portHttpEndpointProvider,
        VolumeProvider volumeProvider,
        DockerService dockerService
    )
    {
        _dockerService = dockerService;
        AddProvider(applicationProvider);
        AddProvider(containerProvider);
        AddProvider(fileProvider);
        AddProvider(portHttpEndpointProvider);
        AddProvider(mountProvider);
        AddProvider(networkProvider);
        AddProvider(portEndpointProvider);
        AddProvider(volumeProvider);

        var traefik = state.Resources.OfType<Application>().FirstOrDefault(a => a.Package?.Name == "traefik");
        if (traefik != null)
        {
            UseTraefik(traefik);
        }
    }

    private void AddProvider(object provider)
    {
        provider.GetType().GetInterfaces()
            .Where(type => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IUninstaller<>))
            .ToList()
            .ForEach(type =>
        {
            var resourceType = type.GetGenericArguments()[0];
            _uninstallers[resourceType] = (IUninstaller)provider;
        });

        provider.GetType().GetInterfaces()
            .Where(type => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IInstaller<>))
            .ToList()
            .ForEach(type =>
                {
                    var contractType = type.GetGenericArguments()[0];
                    
                    if (!_installers.ContainsKey(contractType))
                    {
                        _installers[contractType] = new List<IInstaller>();
                    }
        
                    // Add the provider to the beginning of the list so that it is used first
                    _installers[contractType].Insert(0, (IInstaller)provider);
                }
            );
    }

    public void UseTraefik(Application application)
    {
        var traefikHttpEndpointProvider = new TraefikHttpEndpointProvider(_dockerService, application);
        AddProvider(traefikHttpEndpointProvider);
    }

    public IList<IInstaller> GetInstaller(Type resourceType)
    {
        return _installers[resourceType];
    }
    
    public IUninstaller GetUninstaller(Type resourceType)
    {
        return _uninstallers[resourceType];
    }
}