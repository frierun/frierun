namespace Frierun.Server.Data;

public class ApplicationProvider : Provider<Application, Package>
{
    /// <inheritdoc />
    protected override IEnumerable<ContractDependency> Dependencies(Package package, ExecutionPlan plan)
    {
        return package.Contracts.Select(contract => new ContractDependency(contract, package));
    }
    
    /// <inheritdoc />
    protected override Application Install(Package package, ExecutionPlan plan)
    {
        return new Application(plan.Prefix, package)
        {
            DependsOn = package.Contracts.Select(contract => plan.GetResource(contract.Id)).ToList()
        };
    }

    /// <inheritdoc />
    protected override void Uninstall(Application resource)
    {
    }
}