using Frierun.Server.Models;
using Frierun.Server.Resources;

namespace Frierun.Server.Providers;

public class ApplicationProvider : Provider<Application, Package>
{
    /// <inheritdoc />
    protected override void FillParameters(ExecutionPlan<Package> plan)
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
    protected override bool Validate(ExecutionPlan<Package> plan)
    {
        if (!plan.Parameters.TryGetValue("name", out var name))
        {
            return false;
        }
        return plan.State.Resources.OfType<Application>().All(application => application.Name != name);
    }

    /// <inheritdoc />
    protected override Application Install(ExecutionPlan<Package> plan)
    {
        var name = plan.Parameters["name"];

        return new Application(Guid.NewGuid(), name, plan.Definition)
        {
            DependsOn = plan.InstallChildren()
        };
    }

    /// <inheritdoc />
    protected override void Uninstall(Application resource)
    {
    }
}