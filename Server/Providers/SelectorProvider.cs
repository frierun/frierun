using Frierun.Server.Models;
using Frierun.Server.Resources;

namespace Frierun.Server.Providers;

public class SelectorProvider: Provider
{
    /// <inheritdoc />
    public override ExecutionPlan CreatePlan(State state, ResourceDefinition definition, ExecutionPlan? parent = null)
    {
        return new ExecutionPlan(state, this, parent)
        {
            Parameters =
            {
                ["selected"] = "0"
            }
        };
    }

    /// <inheritdoc />
    public override bool Validate(ExecutionPlan plan)
    {
        if (!plan.Parameters.TryGetValue("selected", out var selected))
        {
            return false;
        }
        
        var intSelected = int.Parse(selected);
        if (intSelected < 0 || intSelected > plan.Children.Count - 1)
        {
            return false;
        }

        return true;
    }

    /// <inheritdoc />
    public override Resource Install(ExecutionPlan plan)
    {
        var selected = int.Parse(plan.Parameters["selected"]);
        return plan.Children[selected].Install();
    }
}