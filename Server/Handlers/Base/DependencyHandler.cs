using Frierun.Server.Data;

namespace Frierun.Server.Handlers.Base;

public class DependencyHandler : Handler<Dependency>
{
    public override IEnumerable<ContractInitializeResult> Initialize(Dependency contract, string prefix)
    {
        yield return new ContractInitializeResult(
            contract with
            {
                DependsOn = [contract.Preceding],
                DependencyOf = [contract.Following],
                Handler = this
            }
        );
    }
}