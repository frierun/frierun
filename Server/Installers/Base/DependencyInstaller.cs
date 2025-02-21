using Frierun.Server.Data;
using Frierun.Server.Services;

namespace Frierun.Server.Installers.Base;

public class DependencyInstaller(ContractRegistry contractRegistry) : IInstaller<Dependency>
{
    /// <inheritdoc />
    public IEnumerable<ContractDependency> Dependencies(Dependency contract, ExecutionPlan plan)
    {
        yield return new ContractDependency(
            contractRegistry.CreateContract(contract.Preceding),
            contractRegistry.CreateContract(contract.Following)
        );
    }
}