namespace Frierun.Server.Data;

public class ApplicationProvider : Provider<Application, Package>
{
    /// <inheritdoc />
    protected override IEnumerable<ContractDependency> Dependencies(Package package, ExecutionPlan plan)
    {
        return package.Contracts.Select(contract => new ContractDependency(contract, package));
    }

    /// <inheritdoc />
    protected override Package Initialize(Package contract, ExecutionPlan plan)
    {
        var count = 0;
        var basePrefix = contract.Prefix ?? contract.Name;
        var prefix = basePrefix;
        while (plan.State.Resources.OfType<Application>().Any(application => application.Name == prefix))
        {
            count++;
            prefix = $"{basePrefix}{count}";
        }

        return prefix == contract.Prefix
            ? contract
            : contract with
            {
                Prefix = prefix
            };
    }

    /// <inheritdoc />
    protected override Application Install(Package package, ExecutionPlan plan)
    {
        return new Application(package.Prefix!, package)
        {
            DependsOn = package.Contracts.Select(contract => plan.GetResource(contract.Id)).ToList()
        };
    }

    /// <inheritdoc />
    protected override void Uninstall(Application resource)
    {
    }
}