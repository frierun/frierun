using Autofac.Features.Indexed;
using Frierun.Server.Data;

namespace Frierun.Server.Services;

public class InstallerRegistry
{
    private readonly IIndex<string, IEnumerable<Func<Application, IInstaller>>> _installerFactories;
    private readonly Dictionary<Application, IList<IInstaller>> _applicationInstallers = new();
    private readonly Dictionary<Type, IList<IInstaller>> _installers = new();
    private readonly Dictionary<Type, IUninstaller> _uninstallers = new();

    public InstallerRegistry(
        State state,
        IEnumerable<IInstaller> staticInstallers,
        IIndex<string, IEnumerable<Func<Application, IInstaller>>> installerFactories
    )
    {
        _installerFactories = installerFactories;

        foreach (var installer in staticInstallers)
        {
            AddInstaller(installer);
        }

        foreach (var application in state.Resources.OfType<Application>())
        {
            AddApplication(application);
        }

        state.ResourceAdded += resource =>
        {
            if (resource is Application application)
            {
                AddApplication(application);
            }
        };

        state.ResourceRemoved += resource =>
        {
            if (resource is Application application)
            {
                RemoveApplication(application);
            }
        };
    }

    /// <summary>
    /// Adds installers from the application to the registry.
    /// </summary>
    private void AddApplication(Application application)
    {
        var packageName = application.Package?.Name;
        if (packageName == null)
        {
            return;
        }

        if (_applicationInstallers.ContainsKey(application))
        {
            throw new Exception("Application already added to the registry");
        }

        var installers = _installerFactories[packageName]
            .Select(installerFactory => installerFactory(application))
            .ToList();

        installers.ForEach(AddInstaller);
        _applicationInstallers[application] = installers;
    }

    /// <summary>
    /// Removes application from the registry.
    /// </summary>
    private void RemoveApplication(Application application)
    {
        var packageName = application.Package?.Name;
        if (packageName == null)
        {
            return;
        }

        if (!_applicationInstallers.TryGetValue(application, out var installers))
        {
            return;
        }

        foreach (var installer in installers)
        {
            _uninstallers
                .Where(pair => pair.Value == installer)
                .ToList()
                .ForEach(pair => _uninstallers.Remove(pair.Key));

            _installers
                .Values
                .ToList()
                .ForEach(list => list.Remove(installer));
        }
    }

    /// <summary>
    /// Adds object to the registry, checking all its interfaces for installers and uninstallers.
    /// </summary>
    private void AddInstaller(object installer)
    {
        installer.GetType().GetInterfaces()
            .Where(type => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IUninstaller<>))
            .ToList()
            .ForEach(
                type =>
                {
                    var resourceType = type.GetGenericArguments()[0];
                    _uninstallers[resourceType] = (IUninstaller)installer;
                }
            );

        installer.GetType().GetInterfaces()
            .Where(type => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IInstaller<>))
            .ToList()
            .ForEach(
                type =>
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

    /// <summary>
    /// Gets specific installer for the resource type or default if name is null
    /// </summary>
    public IInstaller? GetInstaller(Type contractType, string? name = null)
    {
        if (!_installers.TryGetValue(contractType, out var installers))
        {
            return null;
        }

        return name == null
            ? installers.FirstOrDefault()
            : installers.FirstOrDefault(installer => installer.GetType().Name == name);
    }

    /// <summary>
    /// Gets uninstaller for the resource type.
    /// </summary>
    public IUninstaller? GetUninstaller(Type resourceType)
    {
        return _uninstallers.GetValueOrDefault(resourceType);
    }
}