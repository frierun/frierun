namespace Frierun.Server.Data;

public class ApplicationProvider : IInstaller<Package>, IUninstaller<Application>
{
    /// <inheritdoc />
    public IEnumerable<ContractDependency> Dependencies(Package package, ExecutionPlan plan)
    {
        return package.Contracts.Select(contract => new ContractDependency(contract, package));
    }

    /// <inheritdoc />
    public Contract Initialize(Package contract, ExecutionPlan plan)
    {
        var basePrefix = contract.Prefix ?? contract.Name;
        
        var count = 1;
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
    public Resource Install(Package package, ExecutionPlan plan)
    {
        return new Application(package.Prefix!, package)
        {
            DependsOn = package.Contracts
                .Select(contract => plan.GetResource(contract.Id))
                .OfType<Resource>()
                .ToList()
        };
    }

    /// <inheritdoc />
    public void Uninstall(Application resource)
    {
    }
}