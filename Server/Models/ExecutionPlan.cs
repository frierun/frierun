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

    /// <summary>
    /// Installs all children
    /// </summary>
    public IReadOnlyList<Resource> InstallChildren()
    {
        return Children.Select(childPlan => childPlan.Install()).ToList();
    }
}