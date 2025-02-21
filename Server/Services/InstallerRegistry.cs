using Frierun.Server.Data;
using Frierun.Server.Installers.Traefik;

namespace Frierun.Server.Services;

public class InstallerRegistry
{
    private readonly DockerService _dockerService;
    private readonly Dictionary<Type, IList<IInstaller>> _installers = new();
    private readonly Dictionary<Type, IUninstaller> _uninstallers = new();

    public InstallerRegistry(
        State state,
        IEnumerable<IInstaller> installers,
        DockerService dockerService
    )
    {
        _dockerService = dockerService;

        foreach (var installer in installers)
        {
            Add(installer);
        }

        var traefik = state.Resources.OfType<Application>().FirstOrDefault(a => a.Package?.Name == "traefik");
        if (traefik != null)
        {
            UseTraefik(traefik);
        }
    }

    private void Add(object installer)
    {
        installer.GetType().GetInterfaces()
            .Where(type => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IUninstaller<>))
            .ToList()
            .ForEach(type =>
        {
            var resourceType = type.GetGenericArguments()[0];
            _uninstallers[resourceType] = (IUninstaller)installer;
        });

        installer.GetType().GetInterfaces()
            .Where(type => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IInstaller<>))
            .ToList()
            .ForEach(type =>
                {
                    var contractType = type.GetGenericArguments()[0];
                    
                    if (!_installers.ContainsKey(contractType))
                    {
                        _installers[contractType] = new List<IInstaller>();
                    }
        
                    // Add the installer to the beginning of the list so that it is used first
                    _installers[contractType].Insert(0, (IInstaller)installer);
                }
            );
    }

    public void UseTraefik(Application application)
    {
        Add(new TraefikHttpEndpointInstaller(_dockerService, application));
    }

    public IInstaller? GetInstaller(Type resourceType, string? name = null)
    {
        if (!_installers.TryGetValue(resourceType, out var installers))
        {
            return null;
        }

        if (name == null)
        {
            return installers.FirstOrDefault();
        }
        
        return installers.FirstOrDefault(installer => installer.GetType().Name == name);
    }
    
    public IUninstaller GetUninstaller(Type resourceType)
    {
        return _uninstallers[resourceType];
    }
}