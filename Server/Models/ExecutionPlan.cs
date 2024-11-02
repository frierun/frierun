using System.Text.Json.Serialization;
using Frierun.Server.Providers;
using Frierun.Server.Resources;

namespace Frierun.Server.Models;

public class ExecutionPlan<TDefinition>(
    State state,
    TDefinition definition,
    Provider provider,
    ExecutionPlan? parent
) : ExecutionPlan(state, provider, parent)
    where TDefinition : ResourceDefinition
{
    public TDefinition Definition => definition;
}

public class ExecutionPlan(State state, Provider provider, ExecutionPlan? parent)
{
    [JsonIgnore]
    public State State => state;
    
    [JsonIgnore]
    public Provider Provider => provider;
    
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

    public Resource Install()
    {
        var resource = Provider.Install(this);
        State.Resources.Add(resource);
        return resource;
    }

    /// <summary>
    /// Install all children
    /// </summary>
    public IReadOnlyList<Resource> InstallChildren()
    {
        return Children.Select(selector => selector.Selected.Install()).ToList();
    }

    public string GetFullName()
    {
        return Provider.GetFullName(this);
    }
}