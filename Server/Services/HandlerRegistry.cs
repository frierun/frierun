using System.Diagnostics;
using Autofac;
using Autofac.Features.Indexed;
using Frierun.Server.Data;
using Frierun.Server.Handlers;
using Frierun.Server.Handlers.Docker;

namespace Frierun.Server;

public class HandlerRegistry : IDisposable
{
    private readonly IIndex<string, ProviderScopeBuilder> _scopeBuilders;
    private readonly ILifetimeScope _container;
    private readonly ILifetimeScope _baseScope;
    private readonly HashSet<Application> _applicationsToLoad = new();
    private readonly Dictionary<Application, ILifetimeScope> _applicationScopes = new();
    private readonly Dictionary<Application, IList<IHandler>> _handlerPerApplication = new();
    private readonly Dictionary<Type, IList<IHandler>> _handlerPerType = new();
    private readonly Dictionary<(string type, string? application), IHandler> _handlers = new();
    private readonly object _lock = new();

    public HandlerRegistry(
        State state,
        IIndex<string, ProviderScopeBuilder> scopeBuilders,
        ILifetimeScope container
    )
    {
        _scopeBuilders = scopeBuilders;
        _container = container;

        var baseBuilder = _scopeBuilders["base"];
        _baseScope = container.BeginLifetimeScope(builder => baseBuilder(builder));
        foreach (var handler in _baseScope.Resolve<IEnumerable<IHandler>>())
        {
            AddHandler(handler);
        }

        foreach (var application in state.Applications)
        {
            _applicationsToLoad.Add(application);
        }

        state.ApplicationAdded += application => _applicationsToLoad.Add(application);
        state.ApplicationRemoved += RemoveApplication;
    }

    public void Dispose()
    {
        foreach (var applicationScope in _applicationScopes.Values)
        {
            applicationScope.Dispose();
        }

        _baseScope.Dispose();
    }

    /// <summary>
    /// Adds handlers from the application to the registry.
    /// </summary>
    private void AddApplication(Application application)
    {
        lock (_lock)
        {
            _applicationsToLoad.Remove(application);
        
            var packageName = application.Package?.Name;
            if (packageName == null)
            {
                return;
            }

            if (_scopeBuilders.TryGetValue(packageName, out var scopeBuilder) == false)
            {
                return;
            }

            if (_handlerPerApplication.ContainsKey(application))
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

            var handlers = scope.Resolve<IEnumerable<IHandler>>().ToList();

            handlers.ForEach(AddHandler);
            _handlerPerApplication[application] = handlers;
            _applicationScopes[application] = scope;
        }
    }

    /// <summary>
    /// Removes application from the registry.
    /// </summary>
    private void RemoveApplication(Application application)
    {
        lock (_lock)
        {
            _applicationsToLoad.Remove(application);
        
            var packageName = application.Package?.Name;
            if (packageName == null)
            {
                return;
            }

            if (!_handlerPerApplication.Remove(application, out var handlers))
            {
                return;
            }

            foreach (var handler in handlers)
            {
                _handlers.Remove((handler.GetType().Name, application.Name));

                _handlerPerType
                    .Values
                    .ToList()
                    .ForEach(list => list.Remove(handler));
            }

            _applicationScopes[application].Dispose();
            _applicationScopes.Remove(application);
        }
    }
    
    /// <summary>
    /// Adds handler to the registry.
    /// </summary>
    private void AddHandler(IHandler handler)
    {
        _handlers[(handler.GetType().Name, handler.Application?.Name)] = handler;

        var handlerType = handler.GetType().BaseType;
        
        Debug.Assert(handlerType != null);
        
        var contractType = handlerType.GetGenericArguments()[0];

        if (!_handlerPerType.ContainsKey(contractType))
        {
            _handlerPerType[contractType] = new List<IHandler>();
        }

        if (ShouldPrioritize(handler.GetType(), contractType))
        {
            // Add the handler to the beginning of the list so that it is used first
            _handlerPerType[contractType].Insert(0, handler);
        }
        else
        {
            _handlerPerType[contractType].Add(handler);
        }
    }

    /// <summary>
    /// Checks if the handler should be prioritized over others.
    /// </summary>
    private bool ShouldPrioritize(Type handlerType, Type contractType)
    {
        if (handlerType == typeof(TraefikHttpEndpointHandler))
        {
            return true;
        }
        
        if (handlerType == typeof(CloudflareHttpEndpointHandler))
        {
            return true;
        }

        if (handlerType == typeof(NewVolumeHandler))
        {
            return true;
        }
        
        if (contractType.Name.StartsWith("Fake"))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Gets possible handlers for the contract type
    /// </summary>
    public IEnumerable<IHandler> GetHandlers(Type contractType)
    {
        lock (_lock)
        {
            foreach (var application in _applicationsToLoad)
            {
                AddApplication(application);
            }
        
            if (!_handlerPerType.TryGetValue(contractType, out var handlers))
            {
                return Array.Empty<IHandler>();
            }

            return handlers;
        }
    }

    /// <summary>
    /// Gets specific handler
    /// </summary>
    public IHandler? GetHandler(string typeName, string? applicationName = null)
    {
        lock (_lock)
        {
            if (applicationName != null)
            {
                foreach (var application in _applicationsToLoad.Where(application => applicationName == application.Name))
                {
                    AddApplication(application);
                }
            }
            return _handlers.GetValueOrDefault((typeName, applicationName));
        }
    }
}