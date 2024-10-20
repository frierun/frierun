using Frierun.Server.Models;
using Frierun.Server.Resources;

namespace Frierun.Server.Providers;

public class ApplicationProvider : Provider<Application, Package>
{
    /// <inheritdoc />
    protected override void FillParameters(ExecutionPlan<Application, Package> plan)
    {
        plan.Parameters["name"] = plan.Definition.Name;

        var count = 1;
        while (!Validate(plan))
        {
            count++;
            plan.Parameters["name"] = $"{plan.Definition.Name}{count}";
        }
    }
    
    /// <inheritdoc />
    protected override bool Validate(ExecutionPlan<Application, Package> plan)
    {
        if (!plan.Parameters.TryGetValue("name", out var name))
        {
            return false;
        }
        return plan.State.Applications.All(application => application.Name != name);
    }

    /// <inheritdoc />
    protected override Application Install(ExecutionPlan<Application, Package> plan)
    {
        var name = plan.Parameters["name"];
        var resources = plan.Children.Select(childPlan => childPlan.Install()).ToList();

        return new Application(Guid.NewGuid(), name, resources, plan.Definition);
    }
}