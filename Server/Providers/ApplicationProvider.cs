using Frierun.Server.Models;
using Frierun.Server.Resources;

namespace Frierun.Server.Providers;

public class ApplicationProvider : Provider<Application, Package>
{
    /// <inheritdoc />
    protected override void FillParameters(ExecutionPlan<Application, Package> plan)
    {
        var packageName = plan.Definition.Name ?? throw new InvalidOperationException("Package name is required");
        plan.Parameters["name"] = packageName;

        var count = 1;
        while (!Validate(plan))
        {
            count++;
            plan.Parameters["name"] = $"{packageName}-{count}";
        }
    }

    /// <inheritdoc />
    protected override bool Validate(ExecutionPlan<Application, Package> plan)
    {
        if (!plan.Parameters.TryGetValue("name", out var name))
        {
            return false;
        }

        return plan.State.Resources.OfType<Application>().All(application => application.Name != name);
    }

    /// <inheritdoc />
    protected override Application Install(ExecutionPlan<Application, Package> plan)
    {
        var name = plan.Parameters["name"];

        var children = plan.InstallChildren();
        var groupPlans = plan.Children
            .Select(selector => selector.Selected)
            .OfType<ExecutionPlan<ContainerGroup, ContainerGroupDefinition>>();
        foreach (var groupPlan in groupPlans)
        {
            children.AddRange(groupPlan.InstallChildren());
        }

        return new Application(name, plan.Definition)
        {
            DependsOn = children
        };
    }

    /// <inheritdoc />
    protected override void Uninstall(Application resource)
    {
    }
}