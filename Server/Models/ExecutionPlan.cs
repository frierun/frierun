using System.Text.Json.Serialization;
using Frierun.Server.Providers;
using Frierun.Server.Resources;

namespace Frierun.Server.Models;

public class ExecutionPlan<TResource, TDefinition>(
    State state,
    TDefinition definition,
    Provider provider,
    ExecutionPlan? parent
) : ExecutionPlan(state, provider, parent)
    where TResource : Resource
    where TDefinition : ResourceDefinition<TResource>
{
    public TDefinition Definition => definition;

    /// <inheritdoc />
    public override TResource Install()
    {
        return (TResource)base.Install();
    }
}

public class ExecutionPlan(State state, Provider provider, ExecutionPlan? parent)
{
    [JsonIgnore]
    public State State => state;
    
    private Provider Provider => provider;
    private Resource? _resource;
    
    [JsonIgnore]
    public ExecutionPlan? Parent => parent;

    public string TypeName => provider.GetType().Name;
    public IDictionary<string, string> Parameters { get; } = new Dictionary<string, string>();
    public IList<ExecutionPlanSelector> Children { get; } = new List<ExecutionPlanSelector>();

    public bool Validate()
    {
        if (Provider.Validate(this)) return true;

        return Children.All(child => child.Selected.Validate());
    }

    public virtual Resource Install()
    {
        if (_resource == null)
        {
            _resource = Provider.Install(this);
            State.Resources.Add(_resource);
        }

        return _resource;
    }

    /// <summary>
    /// Install all children
    /// </summary>
    public List<Resource> InstallChildren()
    {
        return Children.Select(selector => selector.Selected.Install()).ToList();
    }

    public string GetFullName()
    {
        return Provider.GetFullName(this);
    }
}