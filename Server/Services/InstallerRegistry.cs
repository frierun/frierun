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
    private readonly Dictionary<Application, IList<IHandler>> _handlerPerApplication = new();
    private readonly Dictionary<Type, IList<IHandler>> _handlerPerType = new();
    private readonly Dictionary<InstallerDefinition, IHandler> _handlers = new();

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
        foreach (var handler in _baseScope.Resolve<IEnumerable<IHandler>>())
        {
            AddHandler(handler);
        }

        foreach (var application in state.Applications)
        {
            AddApplication(application);
        }

        state.ApplicationAdded += AddApplication;
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

        if (!_handlerPerApplication.Remove(application, out var handlers))
        {
            return;
        }

        foreach (var handler in handlers)
        {
            _handlers.Remove(new InstallerDefinition(handler.GetType().Name, application.Name));

            _handlerPerType
                .Values
                .ToList()
                .ForEach(list => list.Remove(handler));
        }

        _applicationScopes[application].Dispose();
        _applicationScopes.Remove(application);
    }
    
    /// <summary>
    /// Adds handler to the registry.
    /// </summary>
    private void AddHandler(IHandler handler)
    {
        var definition = new InstallerDefinition(handler.GetType().Name, handler.Application?.Name);
        _handlers[definition] = handler;

        var handlerType = handler
            .GetType()
            .GetInterfaces()
            .Single(type => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IHandler<>));
        
        var contractType = handlerType.GetGenericArguments()[0];

        if (!_handlerPerType.ContainsKey(contractType))
        {
            _handlerPerType[contractType] = new List<IHandler>();
        }

        // Add the handler to the beginning of the list so that it is used first
        _handlerPerType[contractType].Insert(0, handler);
    }

    /// <summary>
    /// Gets possible handlers for the resource type
    /// </summary>
    public IEnumerable<IHandler> GetHandlers(Type contractType)
    {
        if (!_handlerPerType.TryGetValue(contractType, out var handlers))
        {
            return Array.Empty<IHandler>();
        }

        return handlers;
    }

    public IHandler? GetHandler(string typeName, string? applicationName = null)
    {
        var definition = new InstallerDefinition(typeName, applicationName);
        return _handlers.GetValueOrDefault(definition);
    }
}