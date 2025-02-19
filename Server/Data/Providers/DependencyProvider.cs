using Frierun.Server.Services;

namespace Frierun.Server.Data;

public class DependencyProvider(ContractRegistry contractRegistry) : IInstaller<Dependency>
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