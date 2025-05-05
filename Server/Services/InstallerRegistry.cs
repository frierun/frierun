using Autofac;
using Autofac.Features.Indexed;
using Frierun.Server.Data;
using Frierun.Server.Installers;

namespace Frierun.Server;

public class InstallerRegistry : IDisposable
{
    private readonly IIndex<string, ProviderScopeBuilder> _scopeBuilders;
    private readonly ILifetimeScope _container;
    private readonly ILifetimeScope _baseScope;
    private readonly Dictionary<Application, ILifetimeScope> _applicationScopes = new();
    private readonly Dictionary<Application, IList<IInstaller>> _applicationInstallers = new();
    private readonly Dictionary<Type, IList<IInstaller>> _installers = new();
    private readonly Dictionary<Type, IUninstaller> _uninstallers = new();
    private readonly Dictionary<InstallerDefinition, IUninstaller> _handlers = new();

    public InstallerRegistry(
        State state,
        IIndex<string, ProviderScopeBuilder> scopeBuilders,
        ILifetimeScope container
    )
    {
        _scopeBuilders = scopeBuilders;
        _container = container;

        var baseBuilder = _scopeBuilders["base"];
        _baseScope = container.BeginLifetimeScope(builder => baseBuilder(builder));
        foreach (var installer in _baseScope.Resolve<IEnumerable<IInstaller>>())
        {
            AddInstaller(installer);
        }

        foreach (var application in state.Applications)
        {
            AddApplication(application);
        }

        state.ApplicationAdded += AddApplication;
        state.ApplicationRemoved += RemoveApplication;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        foreach (var applicationScope in _applicationScopes.Values)
        {
            applicationScope.Dispose();
        }

        _baseScope.Dispose();
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

        if (_scopeBuilders.TryGetValue(packageName, out var scopeBuilder) == false)
        {
            return;
        }

        if (_applicationInstallers.ContainsKey(application))
        {
            throw new Exception("Application already added to the registry");
        }

        var scope = _container.BeginLifetimeScope(
            _scopeBuilders[packageName],
            builder =>
            {
                builder.RegisterInstance(application).AsSelf().SingleInstance();
                scopeBuilder(builder);
            }
        );

        var installers = scope.Resolve<IEnumerable<IInstaller>>().ToList();

        installers.ForEach(AddInstaller);
        _applicationInstallers[application] = installers;
        _applicationScopes[application] = scope;
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

        if (!_applicationInstallers.Remove(application, out var installers))
        {
            return;
        }

        foreach (var installer in installers)
        {
            _uninstallers
                .Where(pair => pair.Value == installer)
                .ToList()
                .ForEach(pair => _uninstallers.Remove(pair.Key));
            
            _handlers.Remove(new InstallerDefinition(installer.GetType().Name, application.Name));

            _installers
                .Values
                .ToList()
                .ForEach(list => list.Remove(installer));
        }

        _applicationScopes[application].Dispose();
        _applicationScopes.Remove(application);
    }

    /// <summary>
    /// Adds object to the registry, checking all its interfaces for installers and uninstallers.
    /// </summary>
    private void AddInstaller(IInstaller installer)
    {
        installer.GetType().GetInterfaces()
            .Where(type => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IUninstaller<>))
            .ToList()
            .ForEach(
                type =>
                {
                    var resourceType = type.GetGenericArguments()[0];
                    _uninstallers[resourceType] = (IUninstaller)installer;

                    var definition = new InstallerDefinition(installer.GetType().Name, installer.Application?.Name);
                    _handlers[definition] = (IUninstaller)installer;
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
                    _installers[contractType].Insert(0, installer);
                }
            );
    }

    /// <summary>
    /// Gets possible installers for the resource type
    /// </summary>
    public IEnumerable<IInstaller> GetInstallers(Type contractType, InstallerDefinition? definition = null)
    {
        if (!_installers.TryGetValue(contractType, out var installers))
        {
            yield break;
        }

        foreach (var installer in installers)
        {
            if (definition != null && definition.TypeName != installer.GetType().Name)
            {
                continue;
            }

            if (definition?.ApplicationName != null && installer.Application?.Name != definition.ApplicationName)
            {
                continue;
            }

            yield return installer;
        }
    }

    /// <summary>
    /// Gets uninstaller for the resource type.
    /// </summary>
    public IUninstaller? GetUninstaller(Type resourceType)
    {
        return _uninstallers.GetValueOrDefault(resourceType);
    }

    public IUninstaller? GetHandler(InstallerDefinition definition)
    {
        return _handlers.GetValueOrDefault(definition);
    }
}