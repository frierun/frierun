using System.Runtime.Serialization;
using Frierun.Server.Providers;
using Frierun.Server.Resources;

namespace Frierun.Server.Models;

public class ExecutionPlan<TResource, TDefinition>(
    State state,
    TDefinition definition,
    Provider<TResource, TDefinition> provider,
    ExecutionPlan? parent = null
) : ExecutionPlan(state, provider, parent)
    where TResource : Resource
    where TDefinition : ResourceDefinition<TResource>
{
    public TDefinition Definition => definition;
}

public class ExecutionPlan(State state, Provider provider, ExecutionPlan? parent = null)
{
    public State State => state;
    public Provider Provider => provider;
    public ExecutionPlan? Parent => parent;
    
    public IDictionary<string, string> Parameters { get; } = new Dictionary<string, string>();
    public IList<ExecutionPlan> Children { get; } = new List<ExecutionPlan>();

    public bool Validate()
    {
        if (Provider.Validate(this)) return true;
        
        return Children.All(child => child.Validate());
    }
    
    public Resource Install()
    {
        return Provider.Install(this);
    }
}