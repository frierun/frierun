using Frierun.Server.Data;

namespace Frierun.Server.Installers.Base;

public class DependencyInstaller : IInstaller<Dependency>
{
    /// <inheritdoc />
    IEnumerable<ContractDependency> IInstaller<Dependency>.GetDependencies(Dependency contract, ExecutionPlan plan)
    {
        yield return new ContractDependency(contract.Preceding, contract.Following);
    }
}